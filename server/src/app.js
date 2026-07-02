import express from 'express';
import cors from 'cors';
import { v4 as uuid } from 'uuid';
import { ECONOMY } from './economy/config.js';
import { haversineMeters, isWithinRadiusMeters } from './economy/geo.js';
import { rollExtractPoints, rollGatherOutcome, computeKiRequiredSeconds } from './economy/rng.js';
import {
  createGuestPlayer,
  getPlayerByToken,
  resetWalletIfNeeded,
  addInventory,
} from './db/database.js';
import { latLngToH3, getNearbySpawns, ensureHexSpawns } from './world/spawns.js';

export function createApp(db, options = {}) {
  const kiTimeScale = options.kiTimeScale ?? parseFloat(process.env.KI_TIME_SCALE || '1');
  const app = express();
  app.use(cors());
  app.use(express.json());

  function auth(req, res, next) {
    const token = req.headers.authorization?.replace('Bearer ', '');
    if (!token) return res.status(401).json({ error: 'unauthorized' });
    const player = getPlayerByToken(db, token);
    if (!player) return res.status(401).json({ error: 'invalid_token' });
    req.player = player;
    resetWalletIfNeeded(db, player.id);
    next();
  }

  app.post('/auth/guest', (_req, res) => {
    const player = createGuestPlayer(db);
    res.json({ token: player.token, playerId: player.id });
  });

  app.get('/player/wallet', auth, (req, res) => {
    const w = db.prepare('SELECT * FROM player_wallet WHERE player_id = ?').get(req.player.id);
    res.json({
      sourcePoints: w.source_points,
      extractionsRemaining: Math.max(0, ECONOMY.DAILY_EXTRACT_LIMIT - w.extractions_today),
      extractionsToday: w.extractions_today,
    });
  });

  app.get('/player/inventory', auth, (req, res) => {
    const items = db
      .prepare('SELECT * FROM player_inventory WHERE player_id = ?')
      .all(req.player.id);
    res.json({ items });
  });

  app.get('/hex/current', auth, (req, res) => {
    const lat = parseFloat(req.query.lat);
    const lng = parseFloat(req.query.lng);
    if (Number.isNaN(lat) || Number.isNaN(lng)) return res.status(400).json({ error: 'invalid_coords' });
    const h3 = latLngToH3(lat, lng);
    ensureHexSpawns(db, h3);
    const hex = db.prepare('SELECT * FROM hex_cells WHERE h3_index = ?').get(h3);
    const wallet = db.prepare('SELECT * FROM player_wallet WHERE player_id = ?').get(req.player.id);
    res.json({
      h3Index: h3,
      zoneClass: hex?.zone_class ?? 'urban',
      areaModifier: hex?.area_modifier ?? 1,
      canExtract: wallet.extractions_today < ECONOMY.DAILY_EXTRACT_LIMIT,
      extractionsRemaining: Math.max(0, ECONOMY.DAILY_EXTRACT_LIMIT - wallet.extractions_today),
    });
  });

  app.get('/spawns/nearby', auth, (req, res) => {
    const lat = parseFloat(req.query.lat);
    const lng = parseFloat(req.query.lng);
    if (Number.isNaN(lat) || Number.isNaN(lng)) return res.status(400).json({ error: 'invalid_coords' });
    const spawns = getNearbySpawns(db, lat, lng).map((s) => ({
      id: s.id,
      lat: s.lat,
      lng: s.lng,
      spawnType: s.spawn_type,
      subtypeId: s.subtype_id,
      tier: s.tier,
      distanceM: Math.round(haversineMeters(lat, lng, s.lat, s.lng)),
    }));
    res.json({ spawns });
  });

  app.post('/source/extract', auth, (req, res) => {
    const { lat, lng } = req.body;
    if (lat == null || lng == null) return res.status(400).json({ error: 'invalid_coords' });
    const h3 = latLngToH3(lat, lng);
    const wallet = db.prepare('SELECT * FROM player_wallet WHERE player_id = ?').get(req.player.id);
    if (wallet.extractions_today >= ECONOMY.DAILY_EXTRACT_LIMIT) {
      return res.status(429).json({ error: 'daily_limit_reached' });
    }
    const points = rollExtractPoints();
    db.prepare(
      'UPDATE player_wallet SET source_points = source_points + ?, extractions_today = extractions_today + 1 WHERE player_id = ?'
    ).run(points, req.player.id);
    db.prepare(
      'INSERT INTO source_extractions (id, player_id, h3_index, points_awarded, lat, lng) VALUES (?, ?, ?, ?, ?, ?)'
    ).run(uuid(), req.player.id, h3, points, lat, lng);
    const updated = db.prepare('SELECT * FROM player_wallet WHERE player_id = ?').get(req.player.id);
    res.json({
      pointsAwarded: points,
      sourcePoints: updated.source_points,
      extractionsRemaining: Math.max(0, ECONOMY.DAILY_EXTRACT_LIMIT - updated.extractions_today),
    });
  });

  app.post('/resources/gather', auth, (req, res) => {
    const { spawnId, lat, lng } = req.body;
    const wallet = db.prepare('SELECT * FROM player_wallet WHERE player_id = ?').get(req.player.id);
    if (wallet.source_points < ECONOMY.RESOURCE_GATHER_COST) {
      return res.status(402).json({ error: 'insufficient_points', required: ECONOMY.RESOURCE_GATHER_COST });
    }
    const txn = db.transaction(() => {
      const spawn = db.prepare('SELECT * FROM world_spawns WHERE id = ?').get(spawnId);
      if (!spawn) throw { status: 404, error: 'spawn_not_found' };
      if (spawn.state !== 'active') throw { status: 409, error: 'depleted' };
      if (!isWithinRadiusMeters(lat, lng, spawn.lat, spawn.lng, ECONOMY.COLLECTION_RADIUS_M)) {
        throw { status: 403, error: 'out_of_range' };
      }
      db.prepare('UPDATE player_wallet SET source_points = source_points - ? WHERE player_id = ?').run(
        ECONOMY.RESOURCE_GATHER_COST,
        req.player.id
      );
      const outcome = rollGatherOutcome();
      const quantity = outcome.multiplier;
      addInventory(db, req.player.id, 'resource', spawn.subtype_id, spawn.tier, quantity);
      const respawnSec =
        ECONOMY.RESOURCE_RESPAWN_MIN_SECONDS +
        Math.floor(Math.random() * (ECONOMY.RESOURCE_RESPAWN_MAX_SECONDS - ECONOMY.RESOURCE_RESPAWN_MIN_SECONDS));
      const respawnAt = new Date(Date.now() + respawnSec * 1000).toISOString();
      db.prepare(
        "UPDATE world_spawns SET state = 'depleted', depleted_by = ?, respawn_at = ? WHERE id = ?"
      ).run(req.player.id, respawnAt, spawnId);
      db.prepare(
        'INSERT INTO resource_gathers (id, player_id, spawn_id, outcome_tier, multiplier, quantity, points_spent) VALUES (?, ?, ?, ?, ?, ?, ?)'
      ).run(uuid(), req.player.id, spawnId, outcome.tier, outcome.multiplier, quantity, ECONOMY.RESOURCE_GATHER_COST);
      return { outcome, quantity, spawn };
    });
    try {
      const result = txn();
      const updated = db.prepare('SELECT * FROM player_wallet WHERE player_id = ?').get(req.player.id);
      res.json({
        outcomeTier: result.outcome.tier,
        multiplier: result.outcome.multiplier,
        quantity: result.quantity,
        subtypeId: result.spawn.subtype_id,
        sourcePoints: updated.source_points,
      });
    } catch (e) {
      if (e.status) return res.status(e.status).json({ error: e.error });
      throw e;
    }
  });

  app.post('/ki/sessions', auth, (req, res) => {
    const { spawnId, lat, lng, targetKiTier = 1 } = req.body;
    const wallet = db.prepare('SELECT * FROM player_wallet WHERE player_id = ?').get(req.player.id);
    if (wallet.source_points < ECONOMY.KI_START_COST) {
      return res.status(402).json({ error: 'insufficient_points', required: ECONOMY.KI_START_COST });
    }
    const spawn = db.prepare('SELECT * FROM world_spawns WHERE id = ?').get(spawnId);
    if (!spawn || spawn.spawn_type !== 'ki') return res.status(404).json({ error: 'ki_spawn_not_found' });
    if (!isWithinRadiusMeters(lat, lng, spawn.lat, spawn.lng, ECONOMY.COLLECTION_RADIUS_M)) {
      return res.status(403).json({ error: 'out_of_range' });
    }
    const hex = db.prepare('SELECT * FROM hex_cells WHERE h3_index = ?').get(spawn.h3_index);
    const areaMod = hex?.area_modifier ?? 1;
    const seed = spawn.h3_index.length + spawn.subtype_id.length + req.player.id.length;
    let required = computeKiRequiredSeconds(targetKiTier, areaMod, seed);
    required = Math.max(1, Math.floor(required / kiTimeScale));
    const sessionId = uuid();
    const now = new Date().toISOString();
    db.prepare('UPDATE player_wallet SET source_points = source_points - ? WHERE player_id = ?').run(
      ECONOMY.KI_START_COST,
      req.player.id
    );
    db.prepare(
      `INSERT INTO ki_sessions (id, player_id, spawn_id, affinity_id, state, target_ki_tier, required_seconds, elapsed_seconds, area_modifier, source_cost_paid, started_at, last_ping_at, last_lat, last_lng)
       VALUES (?, ?, ?, ?, 'active', ?, ?, 0, ?, ?, ?, ?, ?, ?)`
    ).run(
      sessionId,
      req.player.id,
      spawnId,
      spawn.subtype_id,
      targetKiTier,
      required,
      areaMod,
      ECONOMY.KI_START_COST,
      now,
      now,
      lat,
      lng
    );
    const updated = db.prepare('SELECT * FROM player_wallet WHERE player_id = ?').get(req.player.id);
    res.json({
      sessionId,
      affinityId: spawn.subtype_id,
      requiredSeconds: required,
      elapsedSeconds: 0,
      areaModifier: areaMod,
      sourcePoints: updated.source_points,
    });
  });

  app.post('/ki/sessions/:id/ping', auth, (req, res) => {
    const { lat, lng } = req.body;
    const session = db.prepare('SELECT * FROM ki_sessions WHERE id = ? AND player_id = ?').get(
      req.params.id,
      req.player.id
    );
    if (!session) return res.status(404).json({ error: 'session_not_found' });
    if (session.state === 'completed' || session.state === 'abandoned') {
      return res.json({ state: session.state, elapsedSeconds: session.elapsed_seconds, requiredSeconds: session.required_seconds });
    }
    const spawn = db.prepare('SELECT * FROM world_spawns WHERE id = ?').get(session.spawn_id);
    const now = Date.now();
    const lastPing = new Date(session.last_ping_at).getTime();
    const deltaSec = Math.floor((now - lastPing) / 1000);
    let elapsed = session.elapsed_seconds;
    let state = session.state;
    const inRange = isWithinRadiusMeters(lat, lng, spawn.lat, spawn.lng, ECONOMY.COLLECTION_RADIUS_M);
    if (inRange) {
      if (state === 'paused') state = 'active';
      if (state === 'active') elapsed += deltaSec;
    } else if (state === 'active' || state === 'paused') {
      state = 'paused';
    }
    if (elapsed >= session.required_seconds) {
      state = 'completed';
      addInventory(db, req.player.id, 'ki', session.affinity_id, session.target_ki_tier, session.target_ki_tier);
      db.prepare('UPDATE ki_sessions SET state = ?, elapsed_seconds = ?, completed_at = ?, last_ping_at = ?, last_lat = ?, last_lng = ? WHERE id = ?').run(
        state,
        elapsed,
        new Date().toISOString(),
        new Date().toISOString(),
        lat,
        lng,
        session.id
      );
    } else {
      db.prepare('UPDATE ki_sessions SET state = ?, elapsed_seconds = ?, last_ping_at = ?, last_lat = ?, last_lng = ? WHERE id = ?').run(
        state,
        elapsed,
        new Date().toISOString(),
        lat,
        lng,
        session.id
      );
    }
    res.json({
      state,
      elapsedSeconds: elapsed,
      requiredSeconds: session.required_seconds,
      inRange,
      kiAwarded: state === 'completed' ? session.target_ki_tier : 0,
    });
  });

  app.get('/ki/sessions/active', auth, (req, res) => {
    const session = db
      .prepare("SELECT * FROM ki_sessions WHERE player_id = ? AND state IN ('active', 'paused') ORDER BY started_at DESC LIMIT 1")
      .get(req.player.id);
    res.json({ session: session ?? null });
  });

  app.get('/hex/:h3/info', auth, (req, res) => {
    const hex = db.prepare('SELECT * FROM hex_cells WHERE h3_index = ?').get(req.params.h3);
    const count = db
      .prepare("SELECT COUNT(*) as c FROM world_spawns WHERE h3_index = ? AND state = 'active'")
      .get(req.params.h3);
    res.json({ hex, activeSpawns: count?.c ?? 0 });
  });

  return app;
}
import Database from 'better-sqlite3';
import { SCHEMA_SQL } from './schema.js';
import { v4 as uuid } from 'uuid';

export function createDatabase(path = ':memory:') {
  const db = new Database(path);
  db.pragma('journal_mode = WAL');
  db.exec(SCHEMA_SQL);
  migratePlayersAuthColumns(db);
  return db;
}

function migratePlayersAuthColumns(db) {
  const cols = db.prepare('PRAGMA table_info(players)').all().map((c) => c.name);
  if (!cols.includes('email')) db.exec('ALTER TABLE players ADD COLUMN email TEXT UNIQUE');
  if (!cols.includes('password_hash')) db.exec('ALTER TABLE players ADD COLUMN password_hash TEXT');
}

export function createGuestPlayer(db) {
  return createPlayerWithWallet(db);
}

export function getPlayerByToken(db, token) {
  return db.prepare('SELECT * FROM players WHERE token = ?').get(token);
}

export function getPlayerByEmail(db, email) {
  return db.prepare('SELECT * FROM players WHERE email = ?').get(email);
}

function createPlayerWithWallet(db, { email = null, passwordHash = null } = {}) {
  const id = uuid();
  const token = uuid();
  const today = new Date().toISOString().slice(0, 10);
  db.prepare('INSERT INTO players (id, token, email, password_hash) VALUES (?, ?, ?, ?)').run(
    id,
    token,
    email,
    passwordHash
  );
  db.prepare(
    'INSERT INTO player_wallet (player_id, source_points, extractions_today, extractions_reset_at) VALUES (?, 0, 0, ?)'
  ).run(id, today);
  return { id, token, email };
}

export function createEmailPlayer(db, email, passwordHash) {
  return createPlayerWithWallet(db, { email, passwordHash });
}

export function resetWalletIfNeeded(db, playerId) {
  const today = new Date().toISOString().slice(0, 10);
  const wallet = db.prepare('SELECT * FROM player_wallet WHERE player_id = ?').get(playerId);
  if (wallet && wallet.extractions_reset_at !== today) {
    db.prepare(
      'UPDATE player_wallet SET extractions_today = 0, extractions_reset_at = ? WHERE player_id = ?'
    ).run(today, playerId);
  }
}

export function addInventory(db, playerId, itemType, subtypeId, tier, qty) {
  db.prepare(
    `INSERT INTO player_inventory (player_id, item_type, subtype_id, tier, quantity)
     VALUES (?, ?, ?, ?, ?)
     ON CONFLICT(player_id, item_type, subtype_id, tier)
     DO UPDATE SET quantity = quantity + excluded.quantity`
  ).run(playerId, itemType, subtypeId, tier ?? -1, qty);
}
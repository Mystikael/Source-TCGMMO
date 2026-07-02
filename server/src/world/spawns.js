import { latLngToCell, cellToLatLng } from 'h3-js';
import { v4 as uuid } from 'uuid';
import { KI_AFFINITIES, RESOURCES, tierCapForZone } from '../data/catalog.js';

const H3_RES = 9;

export function latLngToH3(lat, lng) {
  return latLngToCell(lat, lng, H3_RES);
}

export function jitterInCell(h3Index, seed) {
  const [lat, lng] = cellToLatLng(h3Index);
  const r1 = Math.sin(seed * 12.9898) * 43758.5453;
  const r2 = Math.sin((seed + 1) * 78.233) * 12345.6789;
  const dLat = (r1 - Math.floor(r1) - 0.5) * 0.003;
  const dLng = (r2 - Math.floor(r2) - 0.5) * 0.003;
  return { lat: lat + dLat, lng: lng + dLng };
}

export function pickAffinity(hex) {
  let tags = [];
  try {
    tags = JSON.parse(hex.poi_tags || '[]');
  } catch {
    tags = [];
  }
  for (const aff of KI_AFFINITIES) {
    if (tags.some((t) => aff.poiTags.includes(t))) return aff;
  }
  const idx = Math.abs(hashCode(hex.h3_index)) % KI_AFFINITIES.length;
  return KI_AFFINITIES[idx];
}

function hashCode(s) {
  let h = 0;
  for (let i = 0; i < s.length; i++) h = (Math.imul(31, h) + s.charCodeAt(i)) | 0;
  return h;
}

export function pickResource(hex) {
  const cap = hex.max_resource_tier ?? tierCapForZone(hex.zone_class);
  const eligible = RESOURCES.filter((r) => r.tier <= cap);
  const idx = Math.abs(hashCode(hex.h3_index + 'res')) % eligible.length;
  return eligible[idx];
}

export function ensureHexSpawns(db, h3Index) {
  const existing = db
    .prepare("SELECT COUNT(*) as c FROM world_spawns WHERE h3_index = ? AND state = 'active'")
    .get(h3Index);
  if (existing.c > 0) return;

  let hex = db.prepare('SELECT * FROM hex_cells WHERE h3_index = ?').get(h3Index);
  if (!hex) {
    hex = {
      h3_index: h3Index,
      zone_class: 'urban',
      max_resource_tier: 1,
      area_modifier: 1.0,
      poi_tags: '[]',
    };
    db.prepare(
      'INSERT OR IGNORE INTO hex_cells (h3_index, zone_class, max_resource_tier, area_modifier, poi_tags) VALUES (?, ?, ?, ?, ?)'
    ).run(h3Index, hex.zone_class, hex.max_resource_tier, hex.area_modifier, hex.poi_tags);
  }

  const seed = hashCode(h3Index);
  const aff = pickAffinity(hex);
  const posKi = jitterInCell(h3Index, seed);
  db.prepare(
    `INSERT INTO world_spawns (id, h3_index, lat, lng, spawn_type, subtype_id, tier, state)
     VALUES (?, ?, ?, ?, 'ki', ?, NULL, 'active')`
  ).run(uuid(), h3Index, posKi.lat, posKi.lng, aff.id);

  const res = pickResource(hex);
  const posRes = jitterInCell(h3Index, seed + 7);
  db.prepare(
    `INSERT INTO world_spawns (id, h3_index, lat, lng, spawn_type, subtype_id, tier, state)
     VALUES (?, ?, ?, ?, 'resource', ?, ?, 'active')`
  ).run(uuid(), h3Index, posRes.lat, posRes.lng, res.id, res.tier);
}

export function getNearbySpawns(db, lat, lng, radiusM = 500) {
  const h3 = latLngToH3(lat, lng);
  ensureHexSpawns(db, h3);
  const rows = db
    .prepare("SELECT * FROM world_spawns WHERE h3_index = ? AND state = 'active'")
    .all(h3);
  return rows;
}
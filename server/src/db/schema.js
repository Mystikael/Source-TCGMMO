export const SCHEMA_SQL = `
CREATE TABLE IF NOT EXISTS players (
  id TEXT PRIMARY KEY,
  token TEXT NOT NULL UNIQUE,
  email TEXT UNIQUE,
  password_hash TEXT,
  created_at TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS player_wallet (
  player_id TEXT PRIMARY KEY REFERENCES players(id),
  source_points INTEGER NOT NULL DEFAULT 0,
  extractions_today INTEGER NOT NULL DEFAULT 0,
  extractions_reset_at TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS hex_cells (
  h3_index TEXT PRIMARY KEY,
  zone_class TEXT NOT NULL,
  max_resource_tier INTEGER NOT NULL,
  area_modifier REAL NOT NULL DEFAULT 1.0,
  poi_tags TEXT NOT NULL DEFAULT '[]',
  state_code TEXT
);

CREATE TABLE IF NOT EXISTS world_spawns (
  id TEXT PRIMARY KEY,
  h3_index TEXT NOT NULL,
  lat REAL NOT NULL,
  lng REAL NOT NULL,
  spawn_type TEXT NOT NULL,
  subtype_id TEXT NOT NULL,
  tier INTEGER,
  state TEXT NOT NULL DEFAULT 'active',
  depleted_by TEXT,
  respawn_at TEXT,
  created_at TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS ki_sessions (
  id TEXT PRIMARY KEY,
  player_id TEXT NOT NULL REFERENCES players(id),
  spawn_id TEXT NOT NULL REFERENCES world_spawns(id),
  affinity_id TEXT NOT NULL,
  state TEXT NOT NULL,
  target_ki_tier INTEGER NOT NULL DEFAULT 1,
  required_seconds INTEGER NOT NULL,
  elapsed_seconds INTEGER NOT NULL DEFAULT 0,
  area_modifier REAL NOT NULL DEFAULT 1.0,
  source_cost_paid INTEGER NOT NULL DEFAULT 100,
  started_at TEXT NOT NULL,
  completed_at TEXT,
  last_ping_at TEXT NOT NULL,
  last_lat REAL,
  last_lng REAL
);

CREATE TABLE IF NOT EXISTS player_inventory (
  player_id TEXT NOT NULL,
  item_type TEXT NOT NULL,
  subtype_id TEXT NOT NULL,
  tier INTEGER,
  quantity INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY (player_id, item_type, subtype_id, tier)
);

CREATE TABLE IF NOT EXISTS source_extractions (
  id TEXT PRIMARY KEY,
  player_id TEXT NOT NULL,
  h3_index TEXT NOT NULL,
  points_awarded INTEGER NOT NULL,
  lat REAL NOT NULL,
  lng REAL NOT NULL,
  extracted_at TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS resource_gathers (
  id TEXT PRIMARY KEY,
  player_id TEXT NOT NULL,
  spawn_id TEXT NOT NULL,
  outcome_tier TEXT NOT NULL,
  multiplier INTEGER NOT NULL,
  quantity INTEGER NOT NULL,
  points_spent INTEGER NOT NULL,
  rolled_at TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS idx_spawns_h3 ON world_spawns(h3_index);
CREATE INDEX IF NOT EXISTS idx_spawns_state ON world_spawns(state);
`;
import { createDatabase } from './db/database.js';
import { createApp } from './app.js';
import { readFileSync, existsSync } from 'fs';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __dirname = dirname(fileURLToPath(import.meta.url));
const dbPath = process.env.DB_PATH || join(__dirname, '..', 'data', 'source.db');
const db = createDatabase(dbPath);

const pilotPath = join(__dirname, '..', 'data', 'pilot-hexes.json');
if (existsSync(pilotPath)) {
  const hexes = JSON.parse(readFileSync(pilotPath, 'utf8'));
  const insert = db.prepare(
    `INSERT OR REPLACE INTO hex_cells (h3_index, zone_class, max_resource_tier, area_modifier, poi_tags, state_code)
     VALUES (@h3_index, @zone_class, @max_resource_tier, @area_modifier, @poi_tags, @state_code)`
  );
  const load = db.transaction((rows) => rows.forEach((r) => insert.run(r)));
  load(hexes);
  console.log(`Loaded ${hexes.length} pilot hex cells`);
}

const port = process.env.PORT || 3847;
const app = createApp(db);
app.listen(port, () => console.log(`Source TCGMMO server on :${port}`));
/**
 * Pilot geography baker: CA, TX, NY sample hex cells with zone classification.
 * Run: node tools/world-baker/bake-pilot.js
 */
import { createRequire } from 'module';
import { writeFileSync, mkdirSync } from 'fs';
import { dirname, join } from 'path';
import { fileURLToPath } from 'url';

const require = createRequire(import.meta.url);
const { latLngToCell } = require(join(dirname(fileURLToPath(import.meta.url)), '../../server/node_modules/h3-js'));

const __dirname = dirname(fileURLToPath(import.meta.url));
const H3_RES = 9;

const PILOT_LOCATIONS = [
  { name: 'SF Urban', lat: 37.7749, lng: -122.4194, zone: 'urban', tier: 1, mod: 1.0, state: 'CA', tags: ['urban_dense'] },
  { name: 'Golden Gate Park', lat: 37.7694, lng: -122.4862, zone: 'city_park', tier: 3, mod: 1.1, state: 'CA', tags: ['park', 'forest'] },
  { name: 'Yosemite', lat: 37.8651, lng: -119.5383, zone: 'national_park', tier: 8, mod: 1.3, state: 'CA', tags: ['mountain', 'forest'] },
  { name: 'Santa Monica Beach', lat: 34.0195, lng: -118.4912, zone: 'beach', tier: 3, mod: 1.5, state: 'CA', tags: ['coast', 'water'] },
  { name: 'Austin Urban', lat: 30.2672, lng: -97.7431, zone: 'urban', tier: 1, mod: 1.0, state: 'TX', tags: ['urban_dense'] },
  { name: 'Zilker Park', lat: 30.2649, lng: -97.7714, zone: 'city_park', tier: 3, mod: 1.1, state: 'TX', tags: ['park'] },
  { name: 'Big Bend', lat: 29.1275, lng: -103.2428, zone: 'state_park', tier: 5, mod: 1.2, state: 'TX', tags: ['mountain', 'desert'] },
  { name: 'NYC Urban', lat: 40.7128, lng: -74.006, zone: 'urban', tier: 1, mod: 1.0, state: 'NY', tags: ['urban_dense'] },
  { name: 'Central Park', lat: 40.7829, lng: -73.9654, zone: 'city_park', tier: 3, mod: 1.1, state: 'NY', tags: ['park', 'garden'] },
  { name: 'Adirondack', lat: 44.2795, lng: -74.0113, zone: 'state_park', tier: 5, mod: 1.25, state: 'NY', tags: ['forest', 'mountain'] },
];

const hexes = PILOT_LOCATIONS.map((loc) => ({
  h3_index: latLngToCell(loc.lat, loc.lng, H3_RES),
  zone_class: loc.zone,
  max_resource_tier: loc.tier,
  area_modifier: loc.mod,
  poi_tags: JSON.stringify(loc.tags),
  state_code: loc.state,
}));

const outDir = join(__dirname, '..', '..', 'server', 'data');
mkdirSync(outDir, { recursive: true });
const outPath = join(outDir, 'pilot-hexes.json');
writeFileSync(outPath, JSON.stringify(hexes, null, 2));
console.log(`Wrote ${hexes.length} pilot hexes to ${outPath}`);
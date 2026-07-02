import http from 'node:http';
import { writeFileSync } from 'fs';
import { createDatabase } from '../src/db/database.js';
import { createApp } from '../src/app.js';
import { latLngToCell } from 'h3-js';

const SF = { lat: 37.7749, lng: -122.4194 };
const scratch = process.argv[2];

function request(port, method, path, body, token) {
  return new Promise((resolve, reject) => {
    const data = body ? JSON.stringify(body) : null;
    const req = http.request(
      {
        hostname: '127.0.0.1',
        port,
        path,
        method,
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
          ...(data ? { 'Content-Length': Buffer.byteLength(data) } : {}),
        },
      },
      (res) => {
        let raw = '';
        res.on('data', (c) => (raw += c));
        res.on('end', () => resolve({ status: res.statusCode, body: raw ? JSON.parse(raw) : null }));
      }
    );
    req.on('error', reject);
    if (data) req.write(data);
    req.end();
  });
}

const db = createDatabase(':memory:');
const h3 = latLngToCell(SF.lat, SF.lng, 9);
db.prepare(
  `INSERT INTO hex_cells (h3_index, zone_class, max_resource_tier, area_modifier, poi_tags, state_code)
   VALUES (?, 'urban', 1, 1.0, '[]', 'CA')`
).run(h3);

const app = createApp(db);
const server = http.createServer(app);
await new Promise((r) => server.listen(0, '127.0.0.1', r));
const port = server.address().port;

const a = await request(port, 'POST', '/auth/guest');
const b = await request(port, 'POST', '/auth/guest');
for (let i = 0; i < 3; i++) {
  await request(port, 'POST', '/source/extract', SF, a.body.token);
  await request(port, 'POST', '/source/extract', SF, b.body.token);
}
const nearby = await request(port, 'GET', `/spawns/nearby?lat=${SF.lat}&lng=${SF.lng}`, null, a.body.token);
const resource = nearby.body.spawns.find((s) => s.spawnType === 'resource');
const gatherA = await request(
  port,
  'POST',
  '/resources/gather',
  { spawnId: resource.id, lat: resource.lat, lng: resource.lng },
  a.body.token
);
const gatherB = await request(
  port,
  'POST',
  '/resources/gather',
  { spawnId: resource.id, lat: resource.lat, lng: resource.lng },
  b.body.token
);

const evidence = { gatherA, gatherB, spawnId: resource.id };
if (scratch) writeFileSync(scratch, JSON.stringify(evidence, null, 2));
console.log(JSON.stringify(evidence, null, 2));
server.close();
db.close();
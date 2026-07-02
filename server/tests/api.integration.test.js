import { describe, it, before, after } from 'node:test';
import assert from 'node:assert/strict';
import http from 'node:http';
import { createDatabase } from '../src/db/database.js';
import { createApp } from '../src/app.js';
import { latLngToCell } from 'h3-js';

const SF = { lat: 37.7749, lng: -122.4194 };

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
        res.on('end', () => {
          let json = null;
          try {
            json = raw ? JSON.parse(raw) : null;
          } catch {
            json = raw;
          }
          resolve({ status: res.statusCode, body: json });
        });
      }
    );
    req.on('error', reject);
    if (data) req.write(data);
    req.end();
  });
}

describe('API integration', () => {
  let port;
  let server;
  let db;

  after(() => {
    if (server) server.close();
    if (db) db.close();
  });

  before(async () => {
    db = createDatabase(':memory:');
    const h3 = latLngToCell(SF.lat, SF.lng, 9);
    db.prepare(
      `INSERT INTO hex_cells (h3_index, zone_class, max_resource_tier, area_modifier, poi_tags, state_code)
       VALUES (?, 'urban', 1, 1.0, '["water"]', 'CA')`
    ).run(h3);
    const app = createApp(db, { kiTimeScale: 3600 });
    server = http.createServer(app);
    await new Promise((r) => server.listen(0, '127.0.0.1', r));
    port = server.address().port;
  });

  it('guest auth + extract enforces 3/day', async () => {
    const auth = await request(port, 'POST', '/auth/guest');
    assert.equal(auth.status, 200);
    const token = auth.body.token;
    let last;
    for (let i = 0; i < 3; i++) {
      last = await request(port, 'POST', '/source/extract', SF, token);
      assert.equal(last.status, 200);
      assert.ok(last.body.pointsAwarded >= 100 && last.body.pointsAwarded <= 1000);
    }
    const fourth = await request(port, 'POST', '/source/extract', SF, token);
    assert.equal(fourth.status, 429);
    assert.equal(fourth.body.error, 'daily_limit_reached');
  });

  it('gather costs 250 and depletes for second player', async () => {
    const a = await request(port, 'POST', '/auth/guest');
    const b = await request(port, 'POST', '/auth/guest');
    const tokenA = a.body.token;
    const tokenB = b.body.token;
    for (let i = 0; i < 3; i++) {
      await request(port, 'POST', '/source/extract', SF, tokenA);
    }
    const nearby = await request(port, 'GET', `/spawns/nearby?lat=${SF.lat}&lng=${SF.lng}`, null, tokenA);
    const resource = nearby.body.spawns.find((s) => s.spawnType === 'resource');
    assert.ok(resource, 'resource spawn exists');
    const gatherA = await request(
      port,
      'POST',
      '/resources/gather',
      { spawnId: resource.id, lat: resource.lat, lng: resource.lng },
      tokenA
    );
    assert.equal(gatherA.status, 200);
    assert.ok(gatherA.body.quantity >= 1);
    for (let i = 0; i < 3; i++) {
      await request(port, 'POST', '/source/extract', SF, tokenB);
    }
    const gatherB = await request(
      port,
      'POST',
      '/resources/gather',
      { spawnId: resource.id, lat: resource.lat, lng: resource.lng },
      tokenB
    );
    assert.equal(gatherB.status, 409);
    assert.equal(gatherB.body.error, 'depleted');
  });

  it('ki start costs 100 and ping completes with time scale', async () => {
    const auth = await request(port, 'POST', '/auth/guest');
    const token = auth.body.token;
    for (let i = 0; i < 3; i++) {
      await request(port, 'POST', '/source/extract', SF, token);
    }
    const nearby = await request(port, 'GET', `/spawns/nearby?lat=${SF.lat}&lng=${SF.lng}`, null, token);
    const ki = nearby.body.spawns.find((s) => s.spawnType === 'ki');
    assert.ok(ki);
    const start = await request(
      port,
      'POST',
      '/ki/sessions',
      { spawnId: ki.id, lat: ki.lat, lng: ki.lng, targetKiTier: 1 },
      token
    );
    assert.equal(start.status, 200);
    assert.equal(start.body.requiredSeconds, 1);
    await new Promise((r) => setTimeout(r, 1100));
    const ping = await request(
      port,
      'POST',
      `/ki/sessions/${start.body.sessionId}/ping`,
      { lat: ki.lat, lng: ki.lng },
      token
    );
    assert.equal(ping.status, 200);
    assert.equal(ping.body.state, 'completed');
    assert.equal(ping.body.kiAwarded, 1);
  });

  it('ki start blocked without 100 points', async () => {
    const auth = await request(port, 'POST', '/auth/guest');
    const token = auth.body.token;
    const nearby = await request(port, 'GET', `/spawns/nearby?lat=${SF.lat}&lng=${SF.lng}`, null, token);
    const ki = nearby.body.spawns.find((s) => s.spawnType === 'ki');
    const start = await request(
      port,
      'POST',
      '/ki/sessions',
      { spawnId: ki.id, lat: ki.lat, lng: ki.lng },
      token
    );
    assert.equal(start.status, 402);
  });
});
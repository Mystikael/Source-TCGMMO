import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import http from 'node:http';
import { createDatabase } from '../src/db/database.js';
import { createApp } from '../src/app.js';

function request(port, method, path, body) {
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

describe('email auth', () => {
  let server;
  let port;

  it('signup then login returns wallet access', async () => {
    const db = createDatabase();
    server = createApp(db).listen(0);
    port = server.address().port;

    const signup = await request(port, 'POST', '/auth/signup', {
      email: 'Player@Example.com',
      password: 'testpass1',
    });
    assert.equal(signup.status, 201);
    assert.ok(signup.body.token);
    assert.equal(signup.body.email, 'player@example.com');

    const dup = await request(port, 'POST', '/auth/signup', {
      email: 'player@example.com',
      password: 'testpass1',
    });
    assert.equal(dup.status, 409);

    const login = await request(port, 'POST', '/auth/login', {
      email: 'player@example.com',
      password: 'testpass1',
    });
    assert.equal(login.status, 200);
    assert.equal(login.body.token, signup.body.token);

    const bad = await request(port, 'POST', '/auth/login', {
      email: 'player@example.com',
      password: 'wrongpass',
    });
    assert.equal(bad.status, 401);

    server.close();
  });
});
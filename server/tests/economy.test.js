import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { ECONOMY, EXTRACT_WEIGHTS, GATHER_OUTCOMES } from '../src/economy/config.js';
import { haversineMeters, isWithinRadiusMeters } from '../src/economy/geo.js';
import { rollExtractPoints, rollGatherOutcome, computeKiRequiredSeconds } from '../src/economy/rng.js';

describe('economy config constants', () => {
  it('matches alpha spec', () => {
    assert.equal(ECONOMY.DAILY_EXTRACT_LIMIT, 3);
    assert.equal(ECONOMY.KI_START_COST, 100);
    assert.equal(ECONOMY.RESOURCE_GATHER_COST, 250);
    assert.equal(ECONOMY.COLLECTION_RADIUS_M, 40);
  });
});

describe('haversineMeters', () => {
  it('returns ~0 for same point', () => {
    assert.ok(haversineMeters(37.77, -122.42, 37.77, -122.42) < 1);
  });

  it('isWithinRadiusMeters at 30m offset', () => {
    const lat = 37.7749;
    const lng = -122.4194;
    const nearLat = lat + 0.00027;
    assert.ok(isWithinRadiusMeters(lat, lng, nearLat, lng, 40));
    assert.ok(!isWithinRadiusMeters(lat, lng, lat + 0.01, lng, 40));
  });
});

describe('rollExtractPoints', () => {
  it('stays in 100-1000 range', () => {
    for (let i = 0; i < 500; i++) {
      const p = rollExtractPoints();
      assert.ok(p >= 100 && p <= 1000);
    }
  });

  it('majority under 250 over large sample', () => {
    let under250 = 0;
    const n = 5000;
    for (let i = 0; i < n; i++) {
      if (rollExtractPoints() < 250) under250++;
    }
    const pct = under250 / n;
    assert.ok(pct > 0.55, `expected majority under 250, got ${pct}`);
  });
});

describe('rollGatherOutcome', () => {
  it('returns valid outcome tiers', () => {
    const tiers = new Set(GATHER_OUTCOMES.map((o) => o.tier));
    for (let i = 0; i < 200; i++) {
      const o = rollGatherOutcome();
      assert.ok(tiers.has(o.tier));
      assert.ok([1, 2, 3, 10].includes(o.multiplier));
    }
  });
});

describe('computeKiRequiredSeconds', () => {
  it('applies area modifier', () => {
    const base = computeKiRequiredSeconds(1, 1.0, 42);
    const beach = computeKiRequiredSeconds(1, 1.5, 42);
    assert.ok(beach >= base);
  });
});
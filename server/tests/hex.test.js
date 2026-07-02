import { describe, it } from 'node:test';
import assert from 'node:assert/strict';
import { latLngToH3 } from '../src/world/spawns.js';

describe('latLngToH3', () => {
  it('returns stable H3 res-9 index for SF pilot coords', () => {
    const h3 = latLngToH3(37.7749, -122.4194);
    assert.equal(h3, '89283082803ffff');
    assert.equal(latLngToH3(37.7749, -122.4194), h3);
  });

  it('differs for distant coordinates', () => {
    const sf = latLngToH3(37.7749, -122.4194);
    const nyc = latLngToH3(40.7128, -74.006);
    assert.notEqual(sf, nyc);
  });
});
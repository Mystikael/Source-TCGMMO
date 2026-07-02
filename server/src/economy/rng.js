import { EXTRACT_WEIGHTS, GATHER_OUTCOMES } from './config.js';

export function rollExtractPoints(rng = Math.random) {
  const totalWeight = EXTRACT_WEIGHTS.reduce((s, b) => s + b.weight, 0);
  let roll = rng() * totalWeight;
  for (const bucket of EXTRACT_WEIGHTS) {
    roll -= bucket.weight;
    if (roll <= 0) {
      return Math.floor(bucket.min + rng() * (bucket.max - bucket.min + 1));
    }
  }
  return EXTRACT_WEIGHTS[0].min;
}

export function rollGatherOutcome(rng = Math.random) {
  const totalWeight = GATHER_OUTCOMES.reduce((s, o) => s + o.weight, 0);
  let roll = rng() * totalWeight;
  for (const outcome of GATHER_OUTCOMES) {
    roll -= outcome.weight;
    if (roll <= 0) return outcome;
  }
  return GATHER_OUTCOMES[0];
}

export function seededUnit(seed) {
  let x = Math.sin(seed) * 10000;
  return x - Math.floor(x);
}

export function computeKiRequiredSeconds(targetTier, areaModifier, seed) {
  const tiers = {
    1: { min: 30 * 60, max: 60 * 60 },
    2: { min: (3 * 60 + 30) * 60, max: 6 * 60 * 60 },
    3: { min: 6 * 60 * 60, max: 14 * 60 * 60 },
  };
  const t = tiers[targetTier] || tiers[1];
  const base = t.min + Math.floor(seededUnit(seed) * (t.max - t.min + 1));
  return Math.floor(base * (areaModifier || 1));
}
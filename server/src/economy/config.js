export const ECONOMY = {
  DAILY_EXTRACT_LIMIT: 3,
  EXTRACT_MIN: 100,
  EXTRACT_MAX: 1000,
  KI_START_COST: 100,
  RESOURCE_GATHER_COST: 250,
  COLLECTION_RADIUS_M: 40,
  VISIBILITY_RADIUS_M: 500,
  KI_PAUSE_ABANDON_SECONDS: 300,
  KI_REFUND_WINDOW_SECONDS: 300,
  RESOURCE_RESPAWN_MIN_SECONDS: 900,
  RESOURCE_RESPAWN_MAX_SECONDS: 3600,
};

export const EXTRACT_WEIGHTS = [
  { min: 100, max: 249, weight: 70 },
  { min: 250, max: 499, weight: 25 },
  { min: 500, max: 749, weight: 4 },
  { min: 750, max: 1000, weight: 1 },
];

export const GATHER_OUTCOMES = [
  { tier: 'minimum', multiplier: 1, weight: 60 },
  { tier: 'medium', multiplier: 2, weight: 30 },
  { tier: 'maximum', multiplier: 3, weight: 9.99 },
  { tier: 'mythic', multiplier: 10, weight: 0.01 },
];

export const KI_TIER_SECONDS = {
  1: { min: 30 * 60, max: 60 * 60 },
  2: { min: (3 * 60 + 30) * 60, max: 6 * 60 * 60 },
  3: { min: 6 * 60 * 60, max: 14 * 60 * 60 },
};

export const ZONE_TIERS = {
  urban: { min: 0, max: 1 },
  city_park: { min: 2, max: 3 },
  state_park: { min: 4, max: 5 },
  national_park: { min: 6, max: 8 },
  wilderness: { min: 2, max: 5 },
};
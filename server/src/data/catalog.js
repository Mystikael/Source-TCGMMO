export const KI_AFFINITIES = [
  { id: 'fyr', name: 'Fyr', icon: 'Fyr.png', poiTags: ['fire_station', 'industrial'] },
  { id: 'watyr', name: 'Watyr', icon: 'Watyr.png', poiTags: ['water', 'river', 'lake'] },
  { id: 'matyr', name: 'Matyr', icon: 'Matyr.png', poiTags: ['quarry', 'mountain'] },
  { id: 'ayr', name: 'Ayr', icon: 'Ayr.png', poiTags: ['coast', 'wind_farm'] },
  { id: 'aeth', name: 'Aeth', icon: 'Aeth.png', poiTags: ['park', 'forest', 'garden'] },
  { id: 'lux', name: 'Lux', icon: 'Lux.png', poiTags: ['plaza', 'solar_power'] },
  { id: 'vyb', name: 'Vyb', icon: 'Vyb.png', poiTags: ['theatre', 'stadium', 'nightclub'] },
  { id: 'grav', name: 'Grav', icon: 'Grav.png', poiTags: ['observatory', 'urban_dense'] },
  { id: 'psionic', name: 'Psionic', icon: 'Psionic.png', poiTags: ['university', 'library', 'museum'] },
  { id: 'omega', name: 'Omega', icon: 'Omega.png', poiTags: ['fitness_centre', 'gym', 'power_plant'] },
  { id: 'veritas', name: 'Veritas', icon: 'Veritas.png', poiTags: ['place_of_worship', 'courthouse'] },
  { id: 'astral', name: 'Astral', icon: 'Astral.png', poiTags: ['gallery', 'planetarium', 'arts'] },
];

const RESOURCE_NAMES = {
  terra: ['Slate', 'Copir', 'Tinn', 'Iyern', 'Myriite', 'Omnium', 'Rayburst', 'Deminyte', 'Divinium'],
  fauna: ['Critter Pelt', 'Primal Leather', 'Wilderkin Hide', 'Brute Scale', 'Embermane Fur', 'Chimaera skin', 'Stardust Mantle', 'Akshinu Essence', "Furah's Coat"],
  flora: ['Fibren', 'Wyrmwood', "Moa'tah", 'Muushis', 'Yisilthorne', 'Aetherbloom', 'Umbrashade', 'Netherpetal', 'God Root'],
};

export const RESOURCES = [];
for (let tier = 0; tier <= 8; tier++) {
  for (const category of ['terra', 'fauna', 'flora']) {
    RESOURCES.push({
      id: `${category}_t${tier}`,
      tier,
      category,
      name: RESOURCE_NAMES[category][tier],
    });
  }
}

export function tierCapForZone(zoneClass) {
  const caps = {
    urban: 1,
    city_park: 3,
    state_park: 5,
    national_park: 8,
    wilderness: 5,
    beach: 3,
  };
  return caps[zoneClass] ?? 1;
}
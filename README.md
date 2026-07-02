# Source TCGMMO — Location Collection Alpha

**Project name:** Source TCGMMO  
**Repository:** [github.com/Mystikael/Source-TCGMMO](https://github.com/Mystikael/Source-TCGMMO) (GitHub slugs cannot contain spaces; display name is **Source TCGMMO**)

Pokemon Go-style location collection for the Source MMOTCG universe. Walk the US mainland (pilot: CA, TX, NY), earn **Source Points**, gather **ReSources**, and extract **Ki Affinities**.

## Three-Loop Economy

| Action | Cost | Mechanic |
|--------|------|----------|
| **Extract** | Free (3/day) | Walk into any hex → earn 100–1000 Source Points |
| **Gather ReSource** | 250 pts | Gamble: 1x / 2x / 3x / 0.01% Mythic 10x |
| **Extract Ki** | 100 pts | Stay in area; dwell time 30min–8hr by tier |

## Quick Start

### Server
```bash
cd server
npm install
node ../tools/world-baker/bake-pilot.js
npm test
npm start
```

Server runs on `http://127.0.0.1:3847`. Set `KI_TIME_SCALE=3600` for compressed Ki testing.

### Unity Client
1. Open project in Unity 6
2. Menu: **Source TCG → Setup Alpha Scenes** (optional UI polish)
3. Play **Bootstrap** scene (first in build settings)
4. **Sign up** or **log in** with email + password (8+ characters)
5. Uses `GpsSimulator` (SF default); tap Extract / Gather / Start Ki

### Build order
Bootstrap → WorldMap → Inventory

## Project Layout

- `Assets/Source/` — Unity game code
- `Assets/Source/Data/Catalog/` — 12 Ki affinity + 27 resource ScriptableObjects
- `Assets/ki_affinities/` — 12 Ki affinity icon PNGs (referenced by affinity assets)
- `server/` — Shared game server (SQLite alpha)
- `tools/world-baker/` — Pilot hex geography (CA, TX, NY)

## Design Docs

- `1. Source MMOTCG -- Overview .txt`
- `Resources Tier 0 to 8.txt`

## Alpha Scope

Included: hex world, shared spawns, Source/Ki/ReSource loops, guest auth.

Excluded: $ARK blockchain, Sourcery crafting, TCG battles, full AR.
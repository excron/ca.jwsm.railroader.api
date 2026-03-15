# API Agent Notes

This file captures the current `ca.jwsm.railroader.api` checkpoint so future work can pick up the architectural split without re-deriving it.

## Project role

`ca.jwsm.railroader.api` is the shared platform/host layer for Railroader mods.

Current intent:
- the API host owns invasive Harmony hooks into Railroader
- public API contracts describe stable services, events, and extension points
- consumer mods stay thin and submit domain-specific data/logic into the host instead of patching the game directly

## Current live domains

These domains are no longer just scaffolding:
- `trains`
- `ui`
- `persistence`
- `orders`
- `abstractions/World`
- `host/World`

## Important current authority split

### Coupler Forces

The API host now owns:
- save lifecycle timing
- mod-scoped data store access
- typed persistence load/save helpers via `IModDataStore`

Coupler Forces still owns:
- durability schema shape
- wear/break domain logic
- reconciliation against live cars

This is better than the old raw JSON blob boundary, but persistence is still not fully ideal. A likely next step is an API-owned save-scoped typed state/session abstraction so mods stop managing load state so explicitly.

### Map Mod Loader

The API host now owns:
- world layout lifecycle timing
- early world apply hooks
- external asset store registration and base-path resolution for compatibility asset packs

`mapmodloader` now owns:
- UMM entrypoint/UI
- `/MapMods` package discovery
- compatibility naming and manifest parsing
- RailLoader / Strange Customs / Confusing Supplements compatibility runtime
- translation from legacy compatibility concepts into neutral API submissions

This is the intended shape:
- legacy compatibility terms stay in `mapmodloader`
- neutral world/layout/asset authority lives in the API host

## World/map integration checkpoint

Current API-side world integration pieces:
- `abstractions/World/Contracts/IWorldLayoutService.cs`
- `abstractions/World/Contracts/IWorldLayoutResolver.cs`
- `abstractions/World/Contracts/IWorldAssetStoreService.cs`
- `abstractions/World/Models/WorldLayoutSourceUpdate.cs`
- `abstractions/World/Models/WorldAssetStoreRegistration.cs`
- `host/Services/WorldLayoutService.cs`
- `host/Services/WorldAssetStoreService.cs`
- `host/Patches/WorldLifecyclePatch.cs`
- `host/Patches/WorldAssetStorePatch.cs`

Current behavior:
- consumer mods can submit world-layout documents into the host
- consumer mods can submit external asset-store registrations into the host
- the host owns the `PrefabStore` and `AssetPackRuntimeStore` hooks required for those asset packs to resolve at runtime

## Current gaps

The backend now owns world layout and compatibility asset store authority, but the API is still missing a polished public map/world domain.

Not yet standardized as first-class public API:
- terrain modifier contracts
- scenery placement contracts
- river/stream/road spline contracts
- turntable contracts
- foliage removal/replacement contracts
- lighting contracts
- map feature / progression extension contracts
- stable public component-builder registration contracts

So the host has meaningful world authority now, but the public “map mod API” is still incomplete.

## Build + deploy reality

The current test loop assumes:
- UMM mods live in `Railroader/Mods`
- map packages and asset packs live in `Railroader/MapMods`

That split is intentional for this project:
- API host and thin UMM mods stay in `Mods`
- data packages and compatibility assets stay in `MapMods`

Any future compatibility work should preserve that split unless there is hard evidence the game itself cannot support it.

## Recommended next direction

1. keep moving invasive map/runtime hooks from `mapmodloader` into the API host
2. keep legacy compatibility naming contained to `mapmodloader`
3. grow a neutral public map/world API around:
   - topology/layout
   - terrain and masks
   - scenery/assets
   - turntables
   - feature/progression additions
4. revisit persistence with a typed save-scoped session abstraction


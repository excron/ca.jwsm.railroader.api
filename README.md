# ca.jwsm.railroader.api

`ca.jwsm.railroader.api` is the shared host and contract stack for namespaced Railroader UMM/Harmony mods.

The design goal is simple:

- keep fragile Railroader patching in one host mod
- expose stable contracts, services, events, and capabilities to consumer mods
- let gameplay mods consume host-owned seams instead of each mod patching the same game entrypoints

This repository is no longer just a scaffold. It now contains a working host used by live mods such as `ca.jwsm.railroader.mods.couplerforces`.

## Current Host Scope

The installed host mod currently provides:

- save context and save lifecycle events
- mod-scoped save data storage
- train and vehicle lifecycle events
- coupling-attempt interception
- solver constraint telemetry publication
- brake-display discovery events
- coupler tooltip and context-menu provider hooks
- generic coupler menu slot/icon presentation mapping for consumer-provided actions
- vanilla wear/tear state publication
- repair-track repair progress/work publication, including generic repair-work estimation for consumers
- world layout submission and early-apply timing
- external world asset-store registration and base-path resolution
- unload-safe Harmony/bootstrap cleanup
- repeated-log coalescing for spam-prone update and patch paths

## Repository Layout

```text
ca.jwsm.railroader.api/
  README.md
  ca.jwsm.railroader.api.sln
  build/
  docs/
  abstractions/
  core/
  host/
  ui/
  trains/
  orders/
  persistence/
```

## Project Responsibilities

### `abstractions`

Shared public primitives:

- service contracts
- API host access contracts
- capability contracts
- event bus contracts
- diagnostics contracts
- IDs, versions, and result types

### `core`

Generic platform plumbing:

- service registry
- capability registry
- in-memory event bus
- diagnostics fan-out
- API host implementation

### `host`

The only intentionally invasive layer:

- bootstrap and composition
- Harmony patch ownership
- Railroader-native adaptation
- translation from unstable game internals to public contracts

### `ui`

Consumer-facing UI contracts and models:

- HUD hooks
- notifications
- context contribution surfaces

### `trains`

Consumer-facing train, consist, coupler, and repair contracts:

- vehicle/coupler identifiers and context models
- coupler interaction provider surfaces
- train stress contracts
- coupling, telemetry, and repair events
- wear/tear feature contracts

### `orders`

Normalized execution and observation seams for dispatch / AE-style workflows.

### `persistence`

Consumer-facing persistence contracts:

- save context
- save lifecycle
- mod-scoped data storage

## Dependency Rules

Allowed references:

- `core -> abstractions`
- `ui -> abstractions`
- `trains -> abstractions`
- `orders -> abstractions`
- `persistence -> abstractions`
- `host -> abstractions`
- `host -> core`
- `host -> ui`
- `host -> trains`
- `host -> orders`
- `host -> persistence`

Not allowed:

- no cross-domain references between `ui`, `trains`, `orders`, and `persistence`
- consumer mods should not depend on `host`

## Current Consumer Pattern

Consumer mods are expected to:

- require `ca.jwsm.railroader.api`
- query the attached API host through `RailroaderApi.TryGet(...)`
- resolve required services through `apiHost.Services`
- subscribe to host-published events
- register their own providers where appropriate

Example current usage from Coupler Forces:

- consumes save lifecycle and mod data store services
- consumes train integration and wear/tear events
- registers an `ICouplerInteractionProvider`
- registers an `ITrainStressService`

## Build

Typical build:

```powershell
dotnet build .\ca.jwsm.railroader.api.sln -c Release
```

The host project targets `net48` and expects local Railroader / Unity Mod Manager assemblies to be available.

## Deploy

The current deploy scripts live in [build/deploy-to-game.ps1](C:/Users/jsm12/OneDrive/Documents/Game_Projects/Railroader/ca.jwsm.railroader.api/build/deploy-to-game.ps1) and [build/deploy-to-game.bat](C:/Users/jsm12/OneDrive/Documents/Game_Projects/Railroader/ca.jwsm.railroader.api/build/deploy-to-game.bat).

The PowerShell deploy path currently rebuilds and installs:

- `ca.jwsm.railroader.api`
- `ca.jwsm.railroader.mods.couplerforces`
- `ca.jwsm.railroader.mods.locomotivecontrol`
- `ca.jwsm.railroader.mods.compat.mapmodloader`

## Current Status

The host is now carrying real, production-facing integration for save lifecycle, trains, couplers, repair facilities, wear/tear state, and world-layout timing.

Recent hardening work also tightened the host boundary:

- the host no longer encodes Coupler Forces-specific menu action semantics
- the repair facility seam now publishes generic repair estimates instead of forcing consumer mods to reflect `RepairTrack` internals
- save deletion now clears current save context when the deleted save was active

It is still intentionally incomplete as a total modding platform. Some domains remain migration-stage or consumer-owned:

- UMM settings UI still lives in consumer mods
- several gameplay systems are still being migrated behind shared seams
- the world/map surface is usable but not yet a finished general-purpose public map-mod API

## Version Notes

Current host package version:

- `0.1.0.2`

See [docs/CHANGELOG.md](C:/Users/jsm12/OneDrive/Documents/Game_Projects/Railroader/ca.jwsm.railroader.api/docs/CHANGELOG.md) for recent API-host changes.

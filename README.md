# ca.jwsm.railroader.api

`ca.jwsm.railroader.api` is the shared platform and API scaffold for Railroader mods. Its job is to isolate fragile game integration inside a single host layer and expose stable contracts, services, events, and extension points to consumer mods.

## Problem Statement

Railroader mod integrations are easy to make brittle when every mod reaches directly into game internals. This repository is intended to centralize that risk:

- the host mod will eventually patch vanilla Railroader once
- the host will adapt game internals into stable public abstractions
- consumer mods will target stable API projects instead of patching the game themselves

This first version is intentionally architecture-first. It establishes the solution shape, dependency rules, starter contracts, and a minimal host bootstrap without implementing real Railroader integration.

## Design Goals

- keep public contracts small and stable
- keep invasive game adaptation isolated to the host
- allow consumer mods to depend on public API projects, not implementation internals
- keep domains separated so shared types flow through abstractions rather than ad hoc cross-references
- stay lean until real integration requirements are proven

## Repository Structure

Projects live directly under the repository root.

```text
ca.jwsm.railroader.api/
  ca.jwsm.railroader.api.sln
  README.md
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

Pure contracts and shared primitives:

- service interfaces
- API access entrypoint for consumer mods
- event bus contracts
- diagnostics contracts
- common IDs, versions, and result types

This layer should remain free of Harmony, reflection-heavy integration, and Railroader object handling.

### `core`

Generic runtime plumbing shared by the platform:

- API host implementation
- service registry
- capability registry
- in-memory event bus
- diagnostics fan-out

`core` depends only on `abstractions`.

### `host`

The installed host mod:

- owns bootstrap and composition
- will eventually own Harmony patches and native Railroader adaptation
- is the only layer intended to depend on fragile game integration

For v1, this project now starts carrying real host-owned Railroader adaptation for the first migration targets:

- save lifecycle observation
- car add/remove lifecycle publication
- coupling attempt interception
- solver telemetry publication
- brake-display discovery events
- world layout lifecycle and early apply timing
- external world asset-store registration and base-path resolution

### `ui`

Consumer-facing UI contracts and models for:

- windows
- HUD widgets
- context menus
- notifications
- HUD anchor/context seams for locomotive controls and inspector injection

### `trains`

Consumer-facing train, consist, vehicle, and coupler contracts/models/events.

The current scaffold also includes a small train stress contract to support the first Coupler Forces migration without carrying old bridge types forward.

The first DPU migration pass also starts adding train-control seams here:

- selected locomotive/control context
- control request interception contracts
- consist/group topology snapshots

### `orders`

Consumer-facing order execution contracts/models/events for AutoEngineer-style flows:

- submission bridge contracts
- execution observation and state snapshots
- readiness gates evaluated against observed state
- normalized lifecycle events and observer facts

### `persistence`

Consumer-facing persistence contracts/models for:

- save context
- save lifecycle observation
- mod-scoped data storage
- config access

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

If a shared type is needed across domains, it belongs in `abstractions`.

## Host Patch Policy

The host is the only invasive layer. It will eventually patch vanilla Railroader once and translate unstable game internals into stable contracts. Public-facing projects should remain insulated from those implementation details.

That means:

- no Harmony or direct patching in public API projects
- no Railroader-native object handling in public contracts
- no duplicate per-mod patch strategies where the host can provide a stable service instead

## V1 Scope

Included in this scaffold:

- solution and project structure
- shared build defaults
- starter contracts in `abstractions`
- minimal runtime plumbing in `core`
- initial public domain contracts for `ui`, `trains`, `orders`, and `persistence`
- stub host bootstrap and composition root
- starter architecture docs

Deferred for later:

- real Railroader integration
- native object adaptation
- telemetry, routing, ops, gameplay, and multiplayer domains
- backward compatibility shims
- CI, packaging, and installer concerns

## Future Domains

The initial public surface focuses on:

- UI
- trains
- orders
- persistence

Additional domains can be added later once the host integration layer and public contracts prove stable:

- telemetry
- richer world/state public domains
- routing
- operations
- gameplay systems
- multiplayer coordination

## Current Status

This repository is currently an early architecture scaffold. The solution is intended to compile cleanly, document the agreed boundaries, and provide a conservative starting point for future implementation work.

The current `orders` surface now starts to absorb the observer/state direction from legacy Ops Manager work, but the host still exposes only placeholder AE integration until native Railroader observation is implemented.

The current `ui` and `trains` surfaces also now define the first DPU-driven seams for lower-left HUD injection, selected locomotive context, control interception, and consist grouping, but the host does not yet populate those seams from live Railroader state.

The current `persistence` surface now also starts the first Coupler Forces-driven save lifecycle path: the host owns save/load/unload observation and a mod-scoped JSON store so consumer mods can stop patching save lifecycle directly over time.

The current `trains` and `ui` event surfaces now also start the first game-ready Coupler Forces host path: the host publishes vehicle lifecycle, coupling attempt, constraint telemetry, and brake-display availability so the mod can consume shared events instead of owning those Harmony patches itself.

The `trains` surface now also includes a small coupler interaction provider contract so the host can own `CouplerPickable` patching while consumer mods contribute tooltip and menu behavior through the API.

The current `abstractions/World` and `host` surfaces now also start the first real map/runtime migration path: the host owns early world apply timing plus external asset-store hooks, while consumer mods can submit world layout documents and `/MapMods`-backed asset-store registrations into the host. This is the start of a neutral map/world backend, but it is not yet a complete public map-mod API.

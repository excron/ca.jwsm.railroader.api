# Architecture

## Core Rule

The API host owns invasive game entrypoints.

Consumer mods own gameplay behavior.

That boundary is the whole point of this repository.

## Layer Split

### `abstractions`

Stable shared contracts:

- API access
- services
- events
- capabilities
- diagnostics
- shared IDs and result types

### `core`

Generic runtime plumbing:

- service registry
- capability service
- event bus
- diagnostics fan-out
- host shell

### `host`

The only layer intended to patch or adapt Railroader directly:

- Harmony ownership
- native object adaptation
- translation of game state into public contracts
- update-cycle ownership for shared services

### Domain Projects

Public consumer-facing surfaces:

- `ui`
- `trains`
- `orders`
- `persistence`

These projects stay contract-only and do not own game patches.

## Current Shared Boundaries

### Persistence

The host owns:

- save/load/unload/delete observation
- current save context
- mod-scoped save data storage

Consumer mods own:

- what data they save
- when they interpret or reconcile that data

### Trains And Couplers

The host owns:

- vehicle add/remove publication
- coupling attempt interception
- coupling completion publication
- brake-display availability publication
- constraint telemetry capture
- coupler tooltip/menu hosting

Consumer mods own:

- durability systems
- maintenance actions
- force models
- HUD rendering decisions

### Wear / Repair

The host owns:

- vanilla wear/tear option observation
- repair-track repair progress/work publication

Consumer mods own:

- what wear/tear means for their system
- how repair work affects their own durability model

### World

The host owns:

- world layout apply timing
- asset-store registration hooks
- reflection-heavy world notification bridges

Consumer mods own:

- the layout/update data they submit
- their own world-specific authoring or gameplay logic

## Dependency Rule

The public API projects should not drift into game-specific behavior just because they carry the contracts.

If a choice is mod-specific, the host should expose a neutral hook and let the mod decide. A recent example is coupler radial-menu placement: the API host now maps generic menu slots to vanilla quadrants, while Coupler Forces decides which action goes in which slot.

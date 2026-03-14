# V1 Scope

## Included

The v1 scaffold includes these projects:

- `ca.jwsm.railroader.api.abstractions`
- `ca.jwsm.railroader.api.core`
- `ca.jwsm.railroader.api.host`
- `ca.jwsm.railroader.api.ui`
- `ca.jwsm.railroader.api.trains`
- `ca.jwsm.railroader.api.orders`
- `ca.jwsm.railroader.api.persistence`

The included surface area is intentionally small:

- shared contracts, IDs, and result types
- a minimal service registry, event bus, capability service, and diagnostics fan-out
- starter public contracts/models/events for UI, trains, orders, and persistence
- a stub host bootstrap/composition root
- basic build and documentation assets

## Deferred

The following items are explicitly deferred:

- real Railroader integration
- Harmony patches
- reflection-based adaptation and native object binding
- telemetry, world, routing, ops, gameplay, and multiplayer projects
- compatibility shims for older bridges
- CI, release packaging, and installer automation
- deep implementations of storage, diagnostics, UI rendering, or order execution

## Guiding Constraint

If a feature requires speculative design or fragile game coupling to exist, it stays out of v1 until a concrete integration requirement exists.

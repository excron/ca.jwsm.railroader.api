# Architecture

## Layer Split

The solution is organized into four concerns:

### `abstractions`

This is the stable contract layer. It contains:

- public interfaces
- event contracts
- diagnostics contracts
- common IDs and result types

It should remain free of invasive integration concerns.

### `core`

`core` contains generic runtime plumbing that is not specific to Railroader internals:

- service registration and lookup
- event publication and subscription
- capability registration
- diagnostics fan-out

`core` depends only on `abstractions`.

### `host`

`host` is the only layer intended to interact directly with unstable game internals. It owns:

- bootstrap
- platform composition
- future Harmony patching
- future native adaptation

Keeping this logic centralized means public domain contracts can stay stable even when Railroader internals change.

### Domain Projects

The domain projects are consumer-facing API surfaces:

- `ui`
- `trains`
- `orders`
- `persistence`

Each depends only on `abstractions`. They do not reference each other.

## Orders Observer Model

`orders` is where normalized AutoEngineer-style execution flow should live. The public surface now models that flow around:

- `OrderRequest` for submission intent
- `ExecutionState` for normalized lifecycle progression
- `ObserverFact` for host-observed facts such as route acceptance, blockage, arrival, coupling, and manual review
- `ExecutionObservationState` for the host-maintained snapshot that replaces ad hoc per-mod bridge state
- readiness gates that evaluate an order request against current observed state

This keeps consumer mods focused on facts and state transitions instead of arbitrary waits or mod-specific bridge code.

## DPU-Driven HUD And Control Seams

The first DPU migration pass starts a second set of reusable seams across `ui` and `trains`:

- `ui` owns HUD anchor identifiers and HUD context contracts for lower-left locomotive control injection
- `trains` owns selected locomotive context, control request interception, and consist-group topology snapshots

These are intentionally narrow. They exist because the legacy DPU mod currently patches multiple HUD and control entry points just to express one coherent feature set.

## Persistence Lifecycle

The first Coupler Forces persistence pass starts moving save lifecycle ownership into `host` and public persistence contracts:

- `persistence` now defines save lifecycle observation in addition to current save context
- the host owns save/load/unload/application-quit observation
- the host also provides a mod-scoped JSON data store rooted by owner id, save scope, and key

This is meant to pull per-mod save lifecycle patches out of consumer mods and leave the durability/config logic itself mod-owned.

## Train Lifecycle And Telemetry

The Coupler Forces conversion also starts moving train-specific invasive hooks into `host`:

- `host` publishes vehicle add/remove events instead of making the mod patch `TrainController` directly
- `host` intercepts coupling attempts and exposes a mutable attempted-coupling event so consumer mods can request rejection without owning the Harmony patch
- `host` publishes solver constraint telemetry so force-model mods can observe integration corrections without patching `IntegrationSet` themselves
- `host` publishes brake-display availability so overlay consumers can attach without patching `TrainBrakeDisplay` discovery
- `host` also owns coupler pickable tooltip/menu patching and delegates the actual behavior to registered coupler interaction providers

This keeps the Coupler Forces force model and durability logic in the mod while shifting the fragile game-entry ownership into the host.

## Dependency Boundaries

The dependency direction is deliberate:

- `abstractions` has no project dependencies
- `core` depends on `abstractions`
- each domain project depends on `abstractions`
- `host` depends on everything it needs to compose the platform

This keeps shared types centralized and prevents accidental coupling between public domains.

## Why Host Is The Only Invasive Layer

Railroader integration will likely require patching, adaptation, and handling unstable internal types. If that logic spreads across multiple public projects or consumer mods, breakage multiplies quickly.

Centralizing invasive behavior in `host` provides three benefits:

1. patch vanilla Railroader once
2. normalize unstable details behind stable contracts
3. let consumer mods work against services/events instead of internals

That separation is the core design rule of this repository.

The current host implementation only wires placeholder services for `orders`. Real Harmony patches and native planner/notice adaptation still belong here and are intentionally deferred.

The same rule applies to the new DPU-driven seams: the contracts now exist, but live HUD/control/context/topology population still belongs in `host` and remains future work.

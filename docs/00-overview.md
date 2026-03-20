# Overview

`ca.jwsm.railroader.api` is the shared mod platform for namespaced Railroader mods.

The platform has two jobs:

- centralize fragile game integration in a single host mod
- expose stable public contracts, services, events, and provider hooks to consumer mods

The current repository is no longer only an architecture placeholder. It now provides live host-owned integration for:

- save lifecycle and mod-scoped persistence
- train and coupler lifecycle
- coupling attempt interception
- solver telemetry capture
- coupler tooltip / menu hosting
- vanilla wear/tear state publication
- repair-track repair progress/work publication
- world layout lifecycle timing and asset-store hooks

Consumer mods are expected to stay on the contract side of that boundary and keep gameplay-specific behavior in the mod.

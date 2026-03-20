# Current Scope

## Included

The current API stack includes these projects:

- `ca.jwsm.railroader.api.abstractions`
- `ca.jwsm.railroader.api.core`
- `ca.jwsm.railroader.api.host`
- `ca.jwsm.railroader.api.ui`
- `ca.jwsm.railroader.api.trains`
- `ca.jwsm.railroader.api.orders`
- `ca.jwsm.railroader.api.persistence`

The currently implemented host-backed surface includes:

- save context and save lifecycle observation
- mod-scoped save data storage
- vehicle add/remove publication
- coupling attempt interception and veto
- coupling completion events
- integration-set constraint telemetry capture
- brake-display availability events
- coupler tooltip/menu provider hosting
- wear/tear feature state publication
- repair-track progress/work publication
- world layout early-apply timing
- world asset-store registration and base-path resolution
- unload-safe bootstrap and patch cleanup

## Still Consumer-Owned Or Migration-Stage

- UMM settings UI
- gameplay-specific repair/replace logic
- gameplay-specific durability systems
- several higher-level world/map behaviors
- richer dispatch / control / route domains

## Guiding Constraint

If a seam can be shared generically, it should move into the API host.

If a behavior is gameplay-specific, economy-specific, or mod-identity-specific, it should stay in the consumer mod and use the API as a producer/consumer boundary.

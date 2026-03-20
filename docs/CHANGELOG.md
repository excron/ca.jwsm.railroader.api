# Changelog

## 0.1.0.2

This release continues the first real API-host migration pass and tightens host reliability.

- Added coupler interaction hosting so the API owns `CouplerPickable` tooltip and context-menu patching while consumer mods register providers.
- Added wear/tear feature publication through `IWearFeatureService` and `WearAndTearFeatureChangedEvent`.
- Expanded wear/tear publication to include the vanilla wear multiplier and overhaul mileage so consumer mods can scale their own systems from the game's tuning instead of only seeing the on/off toggle.
- Added repair-track publication through `VehicleRepairProgressedEvent` and `VehicleRepairWorkAvailableEvent`.
- Added save-delete lifecycle publication and targeted mod-data deletion for per-save consumer data cleanup.
- Added unload-safe host shutdown so the API detaches cleanly, clears static patch state, and unpatches Harmony on unload.
- Added host-side repeated-log coalescing and fail-soft guards around spam-prone update and patch entry points.
- Refined coupler menu ownership so the API host maps generic menu slots/icons, not mod-specific `Repair` / `Replace` behavior.
- Coupler hover titles can now be built from host/context vehicle display names, letting consumer mods keep generic titles and richer vehicle identification separate from their own tooltip lines.
- Added generic `RepairWorkEstimate` publication so consumer mods can apply repair-facility effects without reflecting `RepairTrack` internals themselves.
- Cleared current save context when an active save is deleted so consumer persistence does not recreate deleted save data on shutdown.
- The generic mod-data store now writes indented JSON so consumer persistence files stay readable.
- Added a generic API-owned overlay text panel service/renderer for lightweight debug or status displays outside mod-owned HUD trees.
- Save-scoped API mod data no longer falls back to a synthetic `default` save id when no real save context exists.

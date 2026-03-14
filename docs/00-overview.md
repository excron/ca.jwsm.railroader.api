# Overview

`ca.jwsm.railroader.api` is a shared mod platform for Railroader. The platform goal is to keep fragile game integration in one host layer and expose stable contracts, services, and events to consumer mods.

This repository starts with a conservative architecture scaffold:

- `abstractions` defines shared contracts and primitives
- `core` provides generic runtime plumbing
- `host` composes the platform and will later own invasive game integration
- `ui`, `trains`, `orders`, and `persistence` expose consumer-facing domains

The current solution intentionally avoids real Railroader bindings. It exists to establish boundaries before deeper implementation begins.

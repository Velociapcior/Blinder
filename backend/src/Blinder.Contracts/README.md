# Blinder.Contracts

Intentionally shared cross-process contracts for the Blinder platform.

## Allowed Content

This library is for **only** types that need to be shared across process boundaries:

- **Request/Response DTOs** for cross-service HTTP communication
- **Event payloads** for message bus integration (future)
- **Shared enumerations** that are part of the public API surface

## Forbidden Content

- **No internal application DTOs** — types used only within a single service belong in that service
- **No domain entities** — business objects live in their respective bounded contexts
- **No business logic** — only plain data structures
- **No infrastructure dependencies** — only primitive types and `Blinder.SharedKernel` references

## Current State

This library is intentionally **empty at this stage** (Story 1.1). Types will be added in later stories as cross-process contracts are identified.

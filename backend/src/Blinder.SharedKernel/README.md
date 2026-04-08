# Blinder.SharedKernel

Foundational primitives shared across all Blinder backend projects.

## Allowed Content

This library must contain **only** the following types of primitives:

- **`Result<T>`** — discriminated union for operation outcomes (success/failure)
- **`Error`** — structured error descriptor (code + message)
- **`BlinderException`** — base exception type for domain errors
- **`CorrelationId`** — strongly-typed wrapper for request correlation identifiers

## Forbidden Content

- **No business logic** — no domain features, no feature-specific code
- **No EF Core** or any data-access references
- **No infrastructure concerns** — no HTTP, no messaging, no external service clients
- **No application-layer code** — no MediatR handlers, no validators, no commands/queries

All app projects reference `Blinder.SharedKernel`. Keep it small, stable, and free of dependencies.

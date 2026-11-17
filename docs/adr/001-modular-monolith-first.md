# ADR-001: Modular monolith first, not microservices

- **Status:** Accepted
- **Date:** 2026-11-17

## Context

TicketFlow has several distinct areas (Catalog, Reservations, Ordering, Payments, Notifications, Identity) and hard problems around concurrency and messaging. I am a solo developer with a free-tier-only budget, and the project must run locally with Docker Compose and deploy to free hosts.

## Decision

Build one deployable application organized as a modular monolith. Each module has its own domain model, its own EF Core DbContext and its own database schema. Modules talk through public contracts and domain/integration events, never through each other's internals. Architecture tests enforce these boundaries.

## Alternatives considered

- **Microservices from day one:** real service boundaries, but each service adds its own deployment, networking, observability and failure modes. For one developer on free hosting this would consume time that should go into the booking, outbox and resilience problems this project exists to demonstrate.
- **Classic layered mon[.gitignore](../../.gitignore)olith without module boundaries:** fastest to start, but boundaries erode and it hides the design decisions I want to show.

## Consequences

- **Good:** one pipeline, one deployment, simple local setup, in-process calls are fast and easy to test, and transactions within a module stay simple.
- **Good:** module boundaries and schema-per-module make a later split into services possible (for example Notifications, which already communicates through RabbitMQ).
- **Cost:** a single deployable unit scales as one; a bug in one module can affect the whole process.
- **Cost:** boundaries rely on discipline, so architecture tests (NetArchTest) are required, not optional.

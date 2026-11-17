# TicketFlow

An event ticketing and seat reservation platform built to handle the hard part of ticketing: many people trying to buy the same seats at the same time.

> **Headline claim:** 1,000 concurrent users competing for 100 seats result in zero overselling, verified by an NBomber load test. The report will be linked here once it exists.

## Pitch

Customers browse events, hold seats for a limited time (default 10 minutes, configurable), pay in Stripe test mode and receive their ticket by email. Admins create events. Under the hood the project focuses on concurrency-safe reservations, reliable messaging (outbox pattern with RabbitMQ), resilient external calls, and a full test pyramid.

## v1 scope

- Browse events and see seat availability
- Hold seats for a limited time (default 10 minutes, configurable).
- Pay with Stripe (test mode)
- Receive an email with a ticket
- Admin creates events

## After v1

Intentionally out of scope for now:

- Azure deployment (Azure-ready pieces will be written but not deployed)
- Multi-currency
- Seat map editor
- Refunds

## Tech

- **Backend:** .NET, C#, FastEndpoints, EF Core, PostgreSQL
- **Architecture:** modular monolith, DDD, clean architecture, outbox pattern
- **Messaging:** RabbitMQ with MassTransit
- **Cache:** Redis
- **Auth:** JWT with refresh token rotation and reuse detection
- **Resilience:** retries and circuit breaker (Microsoft.Extensions.Http.Resilience)
- **Frontend:** Angular 20
- **Testing:** xUnit, Testcontainers, NetArchTest, NBomber
- **CI/CD:** GitHub Actions, GHCR, free-tier hosting
- **Local environment:** Docker Compose

## Architecture decisions

- [ADR-001: Modular monolith first, not microservices](docs/adr/001-modular-monolith-first.md)

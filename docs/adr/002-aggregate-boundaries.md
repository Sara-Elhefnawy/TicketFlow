# ADR-002: Aggregate boundaries — Reservation and Order are separate aggregates

- **Status:** Accepted
- **Date:** 2026-11-24

## Context

A customer picks seats, holds them for a short time, then pays. Two concerns are
involved: seat inventory (who may hold which seat, for how long) and money
(what is owed, whether it was paid). We must decide whether this is one
aggregate or two.

## Decision

`Reservation` and `Order` are separate aggregates in separate modules
(Reservations, Ordering). They reference each other by ID only
(`Order.ReservationId`) and coordinate through domain events
(`ReservationCompleted`, `OrderPaid`, `OrderExpired`).

## Reasons

- **Different lifecycles.** A reservation lives 10 minutes. An order lives as
  long as the business needs payment and refund history.
- **Different invariants.** Reservation protects "a seat is never held twice
  and holds expire". Order protects "only valid payment state transitions".
  Neither invariant needs the other aggregate's data to be checked.
- **Different transactional/contention profile.** Seat holds are highly
  contended; orders are not. Separate aggregates mean separate transactions,
  so contention on seats never locks order rows.
- **Different reasons to change.** Hold duration and expiry rules change
  independently of payment rules.

## Alternatives considered

- **One `Booking` aggregate containing seats and payment.** Rejected: large,
  highly contended, mixes inventory and money rules, and would hold a lock for
  the full life of the order.
- **Order owns the held seats.** Rejected: the hold must exist before any
  order does, and it expires on a different clock.

## Consequences

- Cross-aggregate consistency is eventual and event-driven. Event handlers must
  be idempotent.
- "No seat held twice" cannot be checked by a single Reservation instance, so
  it is enforced in two layers: the domain rule (the caller supplies the
  currently unavailable seats) and a unique database constraint on active
  holds as the final guard against concurrent requests.
- If payment fails or the order expires, a handler must expire/release the
  reservation. This may later need a saga/process manager.
- Time is passed in explicitly (`nowUtc`) so rules are deterministic in tests.

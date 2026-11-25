using TicketFlow.SharedKernel;
using TicketFlow.SharedKernel.ValueObjects;

namespace TicketFlow.Reservations.Domain.Events;

// The user paid in time, so the seats are now truly theirs.
public sealed record ReservationCompleted(Guid ReservationId, Guid ConcertId, IReadOnlyList<SeatNumber> Seats)
    : DomainEvent;

using TicketFlow.SharedKernel;
using TicketFlow.SharedKernel.ValueObjects;

namespace TicketFlow.Reservations.Domain.Events;

// Listeners (e.g. the catalog) use this to free the seats again.
public sealed record ReservationExpired(Guid ReservationId, Guid ConcertId, IReadOnlyList<SeatNumber> Seats)
    : DomainEvent;

using TicketFlow.SharedKernel;
using TicketFlow.SharedKernel.ValueObjects;

namespace TicketFlow.Reservations.Domain.Events;

public sealed record SeatsHeld(
    Guid ReservationId,
    Guid ConcertId,
    Guid UserId,
    IReadOnlyList<SeatNumber> Seats,
    DateTime ExpiresAtUtc) : DomainEvent;

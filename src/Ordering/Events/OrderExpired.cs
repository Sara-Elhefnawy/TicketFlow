using TicketFlow.SharedKernel;

namespace TicketFlow.Ordering.Domain.Events;

public sealed record OrderExpired(Guid OrderId, Guid ReservationId) : DomainEvent;

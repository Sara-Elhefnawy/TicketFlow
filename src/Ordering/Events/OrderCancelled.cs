using TicketFlow.SharedKernel;

namespace TicketFlow.Ordering.Domain.Events;

public sealed record OrderCancelled(Guid OrderId, Guid ReservationId) : DomainEvent;

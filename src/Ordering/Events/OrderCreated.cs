using TicketFlow.SharedKernel;
using TicketFlow.SharedKernel.ValueObjects;

namespace TicketFlow.Ordering.Domain.Events;

public sealed record OrderCreated(Guid OrderId, Guid ReservationId, Money Total) : DomainEvent;

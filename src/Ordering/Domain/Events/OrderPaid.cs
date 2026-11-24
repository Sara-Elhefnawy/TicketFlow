using TicketFlow.SharedKernel;

namespace TicketFlow.Ordering.Domain.Events;

// Reservations/Notifications can react to this: complete the reservation,  send the ticket email.
public sealed record OrderPaid(Guid OrderId, Guid ReservationId) : DomainEvent;

using TicketFlow.Ordering.Domain.Enums;
using TicketFlow.Ordering.Domain.Events;
using TicketFlow.SharedKernel;
using TicketFlow.SharedKernel.ValueObjects;

namespace TicketFlow.Ordering.Domain;

// Note the events reference ReservationId (a Guid), not a Reservation object.
// Aggregates in different modules only know each other by id.
public sealed class Order : AggregateRoot<Guid>
{
    public Guid ReservationId { get; private set; }
    public Money Total { get; private set; } = default!;
    public OrderStatus Status { get; private set; }

    // Until when the customer can pay (matches the reservation hold time).
    public DateTime PaymentDeadlineUtc { get; private set; }

    private Order() {}

    public static Order Create(Guid reservationId, Money total, DateTime paymentDeadlineUtc)
    {
        if (total.Amount <= 0)
            throw new DomainException("Order total must be greater than zero.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ReservationId = reservationId,
            Total = total,
            PaymentDeadlineUtc = paymentDeadlineUtc,
            Status = OrderStatus.Pending
        };

        order.Raise(new OrderCreated(order.Id, reservationId, total));
        return order;
    }

    public void MarkPaid(DateTime nowUtc)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException($"Cannot pay an order that is {Status}.");

        if (nowUtc >= PaymentDeadlineUtc)
            throw new DomainException("Order has expired.");

        Status = OrderStatus.Paid;
        Raise(new OrderPaid(Id, ReservationId));
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Paid)
            throw new DomainException("A paid order cannot be cancelled.");

        if (Status != OrderStatus.Pending)
            throw new DomainException($"Cannot cancel an order that is {Status}.");

        Status = OrderStatus.Cancelled;
        Raise(new OrderCancelled(Id, ReservationId));
    }

    public void Expire(DateTime nowUtc)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException($"Cannot expire an order that is {Status}.");

        if (nowUtc < PaymentDeadlineUtc)
            throw new DomainException("Payment deadline has not passed yet.");

        Status = OrderStatus.Expired;
        Raise(new OrderExpired(Id, ReservationId));
    }
}

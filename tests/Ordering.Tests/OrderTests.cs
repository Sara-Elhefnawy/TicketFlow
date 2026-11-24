using TicketFlow.Ordering.Domain;
using TicketFlow.Ordering.Domain.Enums;
using TicketFlow.Ordering.Domain.Events;
using TicketFlow.SharedKernel;
using TicketFlow.SharedKernel.ValueObjects;

namespace TicketFlow.Ordering.Tests;

public class OrderTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Deadline = Now.AddMinutes(10);

    private static Order NewOrder() => Order.Create(Guid.NewGuid(), new Money(100, "EGP"), Deadline);

    [Fact]
    public void Create_starts_pending_and_raises_event()
    {
        var order = NewOrder();

        Assert.Equal(OrderStatus.Pending, order.Status);
        /*
        Assert.Equal(order.PaymentDeadlineUtc, Deadline);
        */
        Assert.IsType<OrderCreated>(Assert.Single(order.DomainEvents));
    }

    [Fact]
    public void Create_with_zero_total_throws() =>
        Assert.Throws<DomainException>(() => Order.Create(Guid.NewGuid(), new Money(0, "EGP"), Deadline));

    [Fact]
    public void MarkPaid_before_deadline_succeeds_and_raises_event()
    {
        var order = NewOrder();

        order.MarkPaid(Now.AddMinutes(5));

        Assert.Equal(OrderStatus.Paid, order.Status);
        /*
        Assert.Equal(order.PaymentDeadlineUtc, Deadline);
        */
        Assert.IsType<OrderPaid>(order.DomainEvents.Last());
    }

    [Fact]
    public void Paying_an_expired_order_throws()
    {
        var order = NewOrder();

        Assert.Throws<DomainException>(() => order.MarkPaid(Deadline.AddSeconds(1)));
    }

    [Fact]
    public void Paying_after_status_expired_throws()
    {
        var order = NewOrder();
        order.Expire(Deadline);

        Assert.Throws<DomainException>(() => order.MarkPaid(Deadline.AddSeconds(1)));
    }

    [Fact]
    public void Paying_a_cancelled_order_throws()
    {
        var order = NewOrder();
        order.Cancel();

        Assert.Throws<DomainException>(() => order.MarkPaid(Now));
    }

    [Fact]
    public void Paying_twice_throws()
    {
        var order = NewOrder();
        order.MarkPaid(Now);

        Assert.Throws<DomainException>(() => order.MarkPaid(Now));
    }

    [Fact]
    public void Cancelling_a_paid_order_throws()
    {
        var order = NewOrder();
        order.MarkPaid(Now);

        var cancel = Assert.Throws<DomainException>(() => order.Cancel());
        Assert.Contains("paid order cannot be cancelled", cancel.Message);
    }

    [Fact]
    public void Cancelling_a_pending_order_succeeds_and_raises_event()
    {
        var order = NewOrder();
        order.Cancel();

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.IsType<OrderCancelled>(order.DomainEvents.Last());
    }

    [Fact]
    public void Cancelling_twice_throws()
    {
        var o = NewOrder();
        o.Cancel();

        Assert.Throws<DomainException>(() => o.Cancel());
    }

    [Fact]
    public void Expire_before_deadline_throws()
    {
        var o = NewOrder();
        Assert.Throws<DomainException>(() => o.Expire(Now.AddMinutes(5)));
    }

    [Fact]
    public void Expiring_a_paid_order_throws()
    {
        var o = NewOrder();
        o.MarkPaid(Now);

        Assert.Throws<DomainException>(() => o.Expire(Deadline.AddMinutes(1)));
    }
}

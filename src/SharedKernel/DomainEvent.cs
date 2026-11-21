namespace TicketFlow.SharedKernel;

// A domain event is a fact that already happened ("SeatsHeld", "OrderPaid").
// record so it gives value equality and immutability for free
// abstract so every real event (SeatsHeld, OrderPaid...) inherits it.
public abstract record DomainEvent
{
    // Unique id per event.
    // Later, with the outbox pattern and RabbitMQ, handlers can use it to detect duplicates (idempotency).
    public Guid DomainEventId { get; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

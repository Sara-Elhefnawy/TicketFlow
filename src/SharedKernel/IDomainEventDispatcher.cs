namespace TicketFlow.SharedKernel;

// The domain only declares WHAT it needs ("something that can publish events").
// It doesn't know HOW: in-memory handlers, MediatR, outbox + RabbitMQ... That decision belongs to the infrastructure layer.
// This keeps the domain free of technical dependencies, and lets tests use a simple fake.
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyList<DomainEvent> events, CancellationToken ct = default);
}

namespace TicketFlow.SharedKernel;

// An aggregate root is the "front door" of a cluster of objects that must stay consistent together.
// Outside code only talks to the root (e.g. Order), never directly to things inside it.
// That's what lets the root guarantee its own rules.
// It also owns the list of domain events the aggregate raised.
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents;

    // protected: only the aggregate's own methods can raise events.
    // This guarantees an event is only recorded when the aggregate itself decided that something really happened.
    protected void Raise(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    // Public on purpose: after the infrastructure has dispatched the events, it clears them so they are never sent twice.
    public void ClearDomainEvents() => _domainEvents.Clear();
}

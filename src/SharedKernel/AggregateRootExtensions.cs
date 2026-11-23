namespace TicketFlow.SharedKernel;

public static class AggregateRootExtensions
{
    // The "dispatch, then clear" rule in one reusable place, so every handler in the app does it the same, correct way.
    public static async Task DispatchEventsAsync<TId>(
        this AggregateRoot<TId> root,
        IDomainEventDispatcher dispatcher,
        CancellationToken ct = default) where TId : notnull
    {
        // Copy the list first: we are about to clear the original, and we don't want to iterate a list that changes underneath us.
        var events = root.DomainEvents.ToList();
        if (events.Count == 0) return;

        await dispatcher.DispatchAsync(events, ct);

        // Clear ONLY AFTER dispatch succeeded.
        root.ClearDomainEvents();
    }
}

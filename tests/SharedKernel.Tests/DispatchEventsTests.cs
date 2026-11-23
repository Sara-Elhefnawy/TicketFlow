using Xunit;

namespace TicketFlow.SharedKernel.Tests;

public class DispatchEventsTests
{
    // Tiny test-only types, nested here so they don't pollute the real code.
    // We test the generic mechanism, not a specific aggregate.
    private sealed record TestEvent : DomainEvent;

    private sealed class TestAggregate : AggregateRoot<Guid>
    {
        public TestAggregate() => Id = Guid.NewGuid();
        public void RaiseSomething() => Raise(new TestEvent());
    }

    // A fake dispatcher: records what it received, and can be told to fail.
    // No RabbitMQ or database needed to test the rule.
    private sealed class FakeDispatcher : IDomainEventDispatcher
    {
        public List<DomainEvent> Received { get; } = new();
        public bool ShouldThrow { get; set; }

        public Task DispatchAsync(IReadOnlyList<DomainEvent> events, CancellationToken ct = default)
        {
            if (ShouldThrow) throw new InvalidOperationException("boom");
            Received.AddRange(events);
            return Task.CompletedTask;
        }
    }

    // RULE: events are sent, then the list is cleared.
    [Fact]
    public async Task Events_are_dispatched_then_cleared()
    {
        var agg = new TestAggregate();
        agg.RaiseSomething();
        var dispatcher = new FakeDispatcher();

        await agg.DispatchEventsAsync(dispatcher);

        Assert.Single(dispatcher.Received);
        Assert.Empty(agg.DomainEvents);
    }

    [Fact]
    public async Task Events_are_kept_when_dispatch_fails()
    {
        var agg = new TestAggregate();
        agg.RaiseSomething();
        var dispatcher = new FakeDispatcher { ShouldThrow = true };

        await Assert.ThrowsAsync<InvalidOperationException>(() => agg.DispatchEventsAsync(dispatcher));

        Assert.Single(agg.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_empties_the_list()
    {
        var agg = new TestAggregate();
        agg.RaiseSomething();

        agg.ClearDomainEvents();

        Assert.Empty(agg.DomainEvents);
    }
}

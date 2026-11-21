namespace TicketFlow.SharedKernel;

// An entity is an object defined by its IDENTITY, not its data.
// Two orders with the same total are still two different orders; the same order is the same order even after its status changes.
// TId is generic so each entity can pick its id type (Guid now, maybe strongly-typed ids later).
public abstract class Entity<TId> where TId : notnull
{
    // protected set: only the entity itself (or subclasses) can assign the id.
    public TId Id { get; protected set; } = default!;

    // Equality by identity: same type + same Id = same entity.
    // The GetType() check prevents an Order and a Reservation with the same Guid from being considered equal.
    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && GetType() == other.GetType() && Id.Equals(other.Id);

    // Must be overridden together with Equals, otherwise hash-based collections (HashSet, Dictionary) behave incorrectly.
    public override int GetHashCode() => Id.GetHashCode();
}

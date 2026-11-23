namespace TicketFlow.SharedKernel.ValueObjects;

// A value object is defined by its VALUES, not an id.
// Two Money objects of 100 EGP are interchangeable; there's no "which one".
// They are immutable: to "change" one you create a new one.
public abstract class ValueObject
{
    // Each value object lists the fields that make it equal to another one.
    // This way the equality logic is written once here, and subclasses only say *what* to compare.
    protected abstract IEnumerable<object?> GetEqualityComponents();

    // Equal if same type and every component matches, in order.
    public override bool Equals(object? obj) =>
        obj is ValueObject other && GetType() == other.GetType() &&
        GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    // Equal objects must have equal hash codes, so I combine all components.
    public override int GetHashCode() => GetEqualityComponents().Aggregate(0, (hash, c) => HashCode.Combine(hash, c));
}

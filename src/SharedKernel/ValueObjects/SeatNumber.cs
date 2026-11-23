namespace TicketFlow.SharedKernel.ValueObjects;

public sealed class SeatNumber : ValueObject
{
    public string Row { get; } // get-only = immutable
    public int Number { get; }

    public SeatNumber(string row, int number)
    {
        if (string.IsNullOrWhiteSpace(row))
            throw new DomainException("Row is required.");

        if (number <= 0)
            throw new DomainException("Seat number must be positive.");

        Row = row.Trim().ToUpperInvariant();
        Number = number;
    }

    public override string ToString() => $"{Row}{Number}";

    // Returns NEW SeatNumber instead of changing this one (immutability).
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Row;
        yield return Number;
    }
}

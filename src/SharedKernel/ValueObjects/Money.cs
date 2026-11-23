namespace TicketFlow.SharedKernel.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; } // get-only = immutable
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new DomainException("Currency must be a 3-letter code.");

        Amount = decimal.Round(amount, 2);

        Currency = currency.Trim().ToUpperInvariant();
    }

    // Returns NEW Money instead of changing this one (immutability).
    public Money Add(Money other) =>
        Currency != other.Currency
            ? throw new DomainException("Currency mismatch.")
            : new Money(Amount + other.Amount, Currency);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

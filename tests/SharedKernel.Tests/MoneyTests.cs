using TicketFlow.SharedKernel.ValueObjects;
using Xunit;

namespace TicketFlow.SharedKernel.Tests;

public class MoneyTests
{
    [Fact]
    public void Negative_amount_throws() => Assert.Throws<DomainException>(() => new Money(-1, "EGP"));

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("EG")]
    [InlineData("EGPP")]
    public void Invalid_currency_throws(string currency) =>
        Assert.Throws<DomainException>(() => new Money(10, currency));

    [Fact]
    public void Equal_by_value_and_currency_is_normalized()
    {
        Assert.Equal(new Money(10, "egp"), new Money(10, "EGP"));
        Assert.NotEqual(new Money(10, "EGP"), new Money(10, "USD"));
    }

    [Fact]
    public void Amount_is_rounded_to_two_decimals() => Assert.Equal(10.13m, new Money(10.126m, "EGP").Amount);

    [Fact]
    public void Add_same_currency_works() =>
        Assert.Equal(new Money(30, "EGP"), new Money(10, "EGP").Add(new Money(20, "EGP")));

    [Fact]
    public void Add_different_currency_throws() =>
        Assert.Throws<DomainException>(() => new Money(10, "EGP").Add(new Money(10, "USD")));
}

using TicketFlow.SharedKernel.ValueObjects;
using Xunit;

namespace TicketFlow.SharedKernel.Tests;

public class SeatNumberTests
{
    [Theory]
    [InlineData("", 1)]
    [InlineData(" ", 1)]
    [InlineData("A", 0)]
    [InlineData("A", -3)]
    public void Invalid_seat_throws(string row, int number) =>
        Assert.Throws<DomainException>(() => new SeatNumber(row, number));

    [Fact]
    public void Equal_by_value_and_row_is_normalized() => Assert.Equal(new SeatNumber("a", 5), new SeatNumber("A", 5));

    [Fact]
    public void Different_seats_are_not_equal() => Assert.NotEqual(new SeatNumber("A", 5), new SeatNumber("A", 6));
}

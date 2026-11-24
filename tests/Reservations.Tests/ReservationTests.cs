using TicketFlow.Reservations.Domain;
using TicketFlow.Reservations.Domain.Enums;
using TicketFlow.Reservations.Domain.Events;
using TicketFlow.SharedKernel;
using TicketFlow.SharedKernel.ValueObjects;
using Xunit;

namespace TicketFlow.Reservations.Tests;

public class ReservationTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static SeatNumber Seat(int n) => new("A", n);

    private static Reservation NewHold(params int[] seatNumbers) =>
        Reservation.Hold(Guid.NewGuid(), Guid.NewGuid(), seatNumbers.Select(Seat).ToArray(),
            new HashSet<SeatNumber>(), // nothing unavailable
            Now);

    // Happy path: shows the expiry is exactly +10 minutes and that an event was recorded.
    [Fact]
    public void Hold_sets_expiry_to_10_minutes_and_raises_event()
    {
        var r = NewHold(1, 2);

        Assert.Equal(ReservationStatus.Active, r.Status);
        Assert.Equal(Now.AddMinutes(10), r.ExpiresAtUtc);
        Assert.IsType<SeatsHeld>(Assert.Single(r.DomainEvents));
    }

    // RULE: double hold. The seat is in the "unavailable" set (someone else
    // holds it), so the hold must fail.
    [Fact]
    public void Hold_throws_when_seat_is_already_held()
    {
        var held = new HashSet<SeatNumber> { Seat(1) };

        var ex = Assert.Throws<DomainException>(() =>
            Reservation.Hold(Guid.NewGuid(), Guid.NewGuid(), [Seat(1)], held, Now));

        Assert.Contains("already held or sold", ex.Message);
    }

    // RULE: sold seat. Also one bad seat among several good ones makes the whole hold fail (all or nothing).
    [Fact]
    public void Hold_throws_when_seat_is_already_sold()
    {
        var sold = new HashSet<SeatNumber> { Seat(2) };

        Assert.Throws<DomainException>(() =>
            Reservation.Hold(Guid.NewGuid(), Guid.NewGuid(), new[] { Seat(1), Seat(2) }, sold, Now));
    }

    [Fact]
    public void Hold_throws_on_duplicate_seats_in_request() => Assert.Throws<DomainException>(() => NewHold(1, 1));

    [Fact]
    public void Hold_throws_when_no_seats() => Assert.Throws<DomainException>(() => NewHold());

    // RULE: expiry boundary. One tick (the smallest time unit) before 10 minutes it is still valid; at exactly 10 minutes it's expired.
    [Fact]
    public void IsExpired_is_false_just_before_and_true_at_ten_minutes()
    {
        var r = NewHold(1);

        Assert.False(r.IsExpired(Now.AddMinutes(10).AddTicks(-1)));
        Assert.True(r.IsExpired(Now.AddMinutes(10)));
    }

    // Expiring too early would free seats while the user is still paying.
    [Fact]
    public void Expire_before_deadline_throws()
    {
        var r = NewHold(1);
        Assert.Throws<DomainException>(() => r.Expire(Now.AddMinutes(5)));
    }

    [Fact]
    public void Expire_after_deadline_sets_status_and_raises_event()
    {
        var r = NewHold(1);

        r.Expire(Now.AddMinutes(10));

        Assert.Equal(ReservationStatus.Expired, r.Status);
        Assert.IsType<ReservationExpired>(r.DomainEvents.Last());
    }

    // Proves the idempotency: a retried background job must not raise the event twice (that would free seats twice).
    [Fact]
    public void Expire_twice_does_not_raise_a_second_event()
    {
        var r = NewHold(1);
        r.Expire(Now.AddMinutes(10));
        r.Expire(Now.AddMinutes(11));

        Assert.Single(r.DomainEvents.OfType<ReservationExpired>());
    }

    [Fact]
    public void Complete_before_expiry_succeeds_and_raises_event()
    {
        var r = NewHold(1);

        r.Complete(Now.AddMinutes(9));

        Assert.Equal(ReservationStatus.Completed, r.Status);
        Assert.IsType<ReservationCompleted>(r.DomainEvents.Last());
    }

    // RULE: you can't complete a hold after its time ran out, even if the background job hasn't marked it Expired yet.
    [Fact]
    public void Complete_after_expiry_throws()
    {
        var r = NewHold(1);
        Assert.Throws<DomainException>(() => r.Complete(Now.AddMinutes(10)));
    }

    // RULE: an already Expired reservation can't be completed.
    [Fact]
    public void Complete_an_expired_reservation_throws()
    {
        var r = NewHold(1);
        r.Expire(Now.AddMinutes(10));

        Assert.Throws<DomainException>(() => r.Complete(Now.AddMinutes(10)));
    }
}

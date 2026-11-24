using TicketFlow.Reservations.Domain.Enums;
using TicketFlow.Reservations.Domain.Events;
using TicketFlow.SharedKernel;
using TicketFlow.SharedKernel.ValueObjects;

namespace TicketFlow.Reservations.Domain;

public sealed class Reservation : AggregateRoot<Guid>
{
    public static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(10);

    public Guid UserId { get; private set; }
    public Guid ConcertId { get; private set; }
    public IReadOnlyList<SeatNumber> Seats { get; private set; } = new List<SeatNumber>();
    public DateTime ExpiresAtUtc { get; private set; }
    public ReservationStatus Status { get; private set; }

    private Reservation() { }

    // WHY unavailableSeats is a parameter: one Reservation object can't see other reservations.
    // The caller (repository or domain service) looks up which seats are held or sold and passes them in, so the rule still lives in the domain and stays easy to unit test.
    // WHY nowUtc is a parameter: the aggregate never calls DateTime.UtcNow itself.
    // Passing time in makes expiry testable without waiting 10 real minutes.
    public static Reservation Hold(
        Guid userId,
        Guid concertId,
        IReadOnlyCollection<SeatNumber> seats,
        IReadOnlySet<SeatNumber> unavailableSeats,
        DateTime nowUtc)
    {
        if (seats.Count == 0)
            throw new DomainException("Select at least one seat.");

        // The same seat twice in one request is a client mistake.
        // This works because SeatNumber has value equality.
        if (seats.Distinct().Count() != seats.Count)
            throw new DomainException("Duplicate seats in request.");

        if (seats.Any(unavailableSeats.Contains))
            throw new DomainException("Seat is already held or sold.");

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConcertId = concertId,
            Seats = seats.ToList(),
            ExpiresAtUtc = nowUtc + HoldDuration,
            Status = ReservationStatus.Active
        };

        reservation.Raise(new SeatsHeld(
            reservation.Id, concertId, userId, reservation.Seats, reservation.ExpiresAtUtc));

        return reservation;
    }

    // ">=" so the hold is expired exactly AT the 10 minute mark, not after.
    public bool IsExpired(DateTime nowUtc) => nowUtc >= ExpiresAtUtc;

    // Called by a background service that frees old holds.
    public void Expire(DateTime nowUtc)
    {
        if (Status != ReservationStatus.Active)
            return;

        if (!IsExpired(nowUtc))
            throw new DomainException("Reservation has not expired yet.");

        Status = ReservationStatus.Expired;
        Raise(new ReservationExpired(Id, ConcertId, Seats));
    }

    public void Complete(DateTime nowUtc)
    {
        if (Status != ReservationStatus.Active)
            throw new DomainException($"Cannot complete a reservation that is {Status}.");

        // Even if the background job hasn't run yet, a hold past its time must not be completable: otherwise someone could "buy" seats that may already have been given to another user.
        if (IsExpired(nowUtc))
            throw new DomainException("Reservation has expired.");

        Status = ReservationStatus.Completed;
        Raise(new ReservationCompleted(Id, ConcertId, Seats));
    }
}

using GamblingBuddies.Models;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Tests;

public class IntegrationTests
{
    [Fact]
    public void Reservation_CanBeSavedToDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("IntegrationDb")
            .Options;

        using var context = new AppDbContext(options);

        var player = new Player
        {
            FirstName = "Jan",
            LastName = "Testowy",
            Email = "test@test.pl",
            Phone = "123456789",
            CreatedAt = DateTime.Now
        };

        context.Players.Add(player);
        context.SaveChanges();

        var reservation = new Reservation
        {
            PlayerId = player.PlayerId,
            GameSessionId = 1,
            ReservationStatusId = 1,
            ReservedAt = DateTime.Now
        };

        context.Reservations.Add(reservation);
        context.SaveChanges();

        var count = context.Reservations.Count();

        Assert.Equal(1, count);
    }
}
using GamblingBuddies.Controllers;
using GamblingBuddies.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Tests
{
    public class ReservationControllerTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private ReservationController CreateController(AppDbContext context)
        {
            var controller = new ReservationController(context);

            controller.TempData = new TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                new MockTempDataProvider()
            );

            return controller;
        }

        [Fact]
        public void Go_Post_ReturnsError_WhenQuantityIsZero()
        {
            var context = CreateContext();
            var controller = CreateController(context);

            var result = controller.Go(1, 1, 1, DateTime.Now.AddDays(1), "12:00", 2, 0, "Cash", "Jan", "Kowalski", "jan@example.com", "123456789");

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Go", redirect.ActionName);
            Assert.Equal("Liczba osób musi być większa od zera.", controller.TempData["Error"]);
        }

        [Fact]
        public void Go_Post_ReturnsError_WhenReservationDateIsInPast()
        {
            var context = CreateContext();
            var controller = CreateController(context);

            var result = controller.Go(1, 1, 1, DateTime.Now.AddDays(-1), "12:00", 2, 2, "Cash", "Jan", "Kowalski", "jan@example.com", "123456789");

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Go", redirect.ActionName);
            Assert.Equal("Nie można utworzyć rezerwacji w przeszłości.", controller.TempData["Error"]);
        }

        [Fact]
        public void Go_Post_ReturnsError_WhenHallGameOrTableIsMissing()
        {
            var context = CreateContext();
            var controller = CreateController(context);

            var result = controller.Go(0, 0, 0, DateTime.Now.AddDays(1), "12:00", 2, 2, "Cash", "Jan", "Kowalski", "jan@example.com", "123456789");

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Go", redirect.ActionName);
            Assert.Equal("Wybierz salę, grę oraz stół.", controller.TempData["Error"]);
        }

        [Fact]
        public void Go_Post_ReturnsError_WhenTableHasOverlappingSession()
        {
            var context = CreateContext();
            var controller = CreateController(context);

            var start = DateTime.Now.AddDays(1).Date.AddHours(12);

            context.Halls.Add(new Hall
            {
                HallId = 1,
                Name = "Main Hall",
                Description = "Test",
                HallTypeId = 1,
                IsActive = true
            });

            context.GameTables.Add(new GameTable
            {
                GameTableId = 1,
                HallId = 1,
                TableNumber = 101,
                MinPlayers = 1,
                MaxPlayers = 6,
                IsActive = true
            });

            context.Games.Add(new Game
            {
                GameId = 1,
                Name = "Poker",
                Description = "Test",
                GameCategoryId = 1,
                IsActive = true
            });

            context.GameVariants.Add(new GameVariant
            {
                GameVariantId = 1,
                GameId = 1,
                Name = "Texas Holdem",
                RulesDescription = "Test",
                DefaultMinBet = 10,
                DefaultMaxBet = 100,
                IsActive = true
            });

            context.SessionStatusDictionaries.Add(new SessionStatusDictionary
            {
                SessionStatusId = 1,
                Name = "Planned",
                Description = "Zaplanowana"
            });

            context.SystemUsers.Add(new SystemUser
            {
                SystemUserId = 1,
                Login = "admin",
                Email = "admin@test.pl",
                PasswordHash = "test",
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            context.GameSessions.Add(new GameSession
            {
                GameSessionId = 1,
                GameVariantId = 1,
                GameTableId = 1,
                StartAt = start,
                EndAt = start.AddHours(3),
                SessionStatusId = 1,
                CreatedByUserId = 1
            });

            context.SaveChanges();

            var result = controller.Go(1, 1, 1, start.Date, "13:00", 2, 2, "Cash", "Jan", "Kowalski", "jan@example.com", "123456789");

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Go", redirect.ActionName);
            Assert.Equal("Wybrany stół nie obsługuje tej gry.", controller.TempData["Error"]);
        }
    }

    public class MockTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(Microsoft.AspNetCore.Http.HttpContext context)
        {
            return new Dictionary<string, object>();
        }

        public void SaveTempData(Microsoft.AspNetCore.Http.HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
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

            var result = controller.Go(
                HallId: 1,
                GameId: 1,
                GameTableId: 1,
                ReservationDate: DateTime.Now.AddDays(1),
                ReservationTime: "12:00",
                DurationHours: 2,
                Quantity: 0,
                PaymentOption: "Cash",
                PlayerFirstName: "Jan",
                PlayerLastName: "Kowalski",
                PlayerEmail: "jan@example.com",
                PlayerPhone: "123456789"
            );

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Go", redirect.ActionName);
            Assert.Equal("Liczba osób musi być większa od zera.", controller.TempData["Error"]);
        }

        [Fact]
        public void Go_Post_ReturnsError_WhenReservationDateIsInPast()
        {
            var context = CreateContext();
            var controller = CreateController(context);

            var result = controller.Go(
                HallId: 1,
                GameId: 1,
                GameTableId: 1,
                ReservationDate: DateTime.Now.AddDays(-1),
                ReservationTime: "12:00",
                DurationHours: 2,
                Quantity: 2,
                PaymentOption: "Cash",
                PlayerFirstName: "Jan",
                PlayerLastName: "Kowalski",
                PlayerEmail: "jan@example.com",
                PlayerPhone: "123456789"
            );

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Go", redirect.ActionName);
            Assert.Equal("Nie można utworzyć rezerwacji w przeszłości.", controller.TempData["Error"]);
        }

        [Fact]
        public void Go_Post_ReturnsError_WhenHallGameOrTableIsMissing()
        {
            var context = CreateContext();
            var controller = CreateController(context);

            var result = controller.Go(
                HallId: 0,
                GameId: 0,
                GameTableId: 0,
                ReservationDate: DateTime.Now.AddDays(1),
                ReservationTime: "12:00",
                DurationHours: 2,
                Quantity: 2,
                PaymentOption: "Cash",
                PlayerFirstName: "Jan",
                PlayerLastName: "Kowalski",
                PlayerEmail: "jan@example.com",
                PlayerPhone: "123456789"
            );

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Go", redirect.ActionName);
            Assert.Equal("Wybierz salę, grę oraz stół.", controller.TempData["Error"]);
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
using GamblingBuddies.Controllers;
using GamblingBuddies.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Tests
{
    public class ReservationsControllerTests
    {
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        private ReservationsController CreateController(AppDbContext context)
        {
            var controller = new ReservationsController(context);

            controller.TempData = new TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                new MockTempDataProvider()
            );

            return controller;
        }

        [Fact]
        public void Confirm_ChangesReservationStatusToConfirmed()
        {
            var context = CreateContext();

            var pending = new ReservationStatusDictionary
            {
                Name = "Pending",
                Description = "Oczekująca"
            };

            var confirmed = new ReservationStatusDictionary
            {
                Name = "Confirmed",
                Description = "Potwierdzona"
            };

            context.ReservationStatusDictionaries.AddRange(pending, confirmed);
            context.SaveChanges();

            var reservation = new Reservation
            {
                PlayerId = 1,
                GameSessionId = 1,
                ReservationStatusId = pending.ReservationStatusId,
                ReservedAt = DateTime.Now
            };

            context.Reservations.Add(reservation);
            context.SaveChanges();

            var controller = CreateController(context);

            var result = controller.Confirm(reservation.ReservationId);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            var updated = context.Reservations.First(r => r.ReservationId == reservation.ReservationId);

            Assert.Equal(confirmed.ReservationStatusId, updated.ReservationStatusId);
            Assert.Equal("Rezerwacja została zatwierdzona.", controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public void Cancel_ChangesReservationStatusToCancelled()
        {
            var context = CreateContext();

            var pending = new ReservationStatusDictionary
            {
                Name = "Pending",
                Description = "Oczekująca"
            };

            var cancelled = new ReservationStatusDictionary
            {
                Name = "Cancelled",
                Description = "Anulowana"
            };

            context.ReservationStatusDictionaries.AddRange(pending, cancelled);
            context.SaveChanges();

            var reservation = new Reservation
            {
                PlayerId = 1,
                GameSessionId = 1,
                ReservationStatusId = pending.ReservationStatusId,
                ReservedAt = DateTime.Now
            };

            context.Reservations.Add(reservation);
            context.SaveChanges();

            var controller = CreateController(context);

            var result = controller.Cancel(reservation.ReservationId);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            var updated = context.Reservations.First(r => r.ReservationId == reservation.ReservationId);

            Assert.Equal(cancelled.ReservationStatusId, updated.ReservationStatusId);
            Assert.Equal("Rezerwacja została anulowana.", controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public void Confirm_ReturnsNotFound_WhenReservationDoesNotExist()
        {
            var context = CreateContext();
            var controller = CreateController(context);

            var result = controller.Confirm(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Cancel_ReturnsNotFound_WhenReservationDoesNotExist()
        {
            var context = CreateContext();
            var controller = CreateController(context);

            var result = controller.Cancel(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
using GamblingBuddies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Controllers
{
    [Authorize(Roles = "Administrator,Manager")]
    public class ReservationsController : Controller
    {
        private readonly AppDbContext _context;

        public ReservationsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var reservations = _context.Reservations
                .Include(r => r.Player)
                .Include(r => r.ReservationStatus)
                .Include(r => r.GameSession)
                    .ThenInclude(gs => gs.GameVariant)
                .Include(r => r.GameSession)
                    .ThenInclude(gs => gs.GameTable)
                        .ThenInclude(gt => gt.Hall)
                .Include(r => r.ReservationSeats)
                    .ThenInclude(rs => rs.Seat)
                .OrderByDescending(r => r.ReservedAt)
                .ToList();

            return View(reservations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirm(int id)
        {
            var reservation = _context.Reservations
                .FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            var confirmedStatus = _context.ReservationStatusDictionaries
                .FirstOrDefault(s => s.Name == "Confirmed");

            if (confirmedStatus == null)
            {
                TempData["ErrorMessage"] = "Brak statusu Confirmed w bazie.";
                return RedirectToAction("Index");
            }

            reservation.ReservationStatusId = confirmedStatus.ReservationStatusId;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Rezerwacja została zatwierdzona.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            var reservation = _context.Reservations
                .FirstOrDefault(r => r.ReservationId == id);

            if (reservation == null)
                return NotFound();

            var cancelledStatus = _context.ReservationStatusDictionaries
                .FirstOrDefault(s => s.Name == "Cancelled");

            if (cancelledStatus == null)
            {
                cancelledStatus = new ReservationStatusDictionary
                {
                    Name = "Cancelled",
                    Description = "Anulowana"
                };

                _context.ReservationStatusDictionaries.Add(cancelledStatus);
                _context.SaveChanges();
            }

            reservation.ReservationStatusId = cancelledStatus.ReservationStatusId;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Rezerwacja została anulowana.";
            return RedirectToAction("Index");
        }
    }
}
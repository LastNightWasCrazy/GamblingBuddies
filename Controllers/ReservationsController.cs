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
                .Include(r => r.GameSession)
                    .ThenInclude(gs => gs.GameVariant)
                .Include(r => r.ReservationStatus)
                .ToList();

            return View(reservations);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Player)
                .Include(r => r.GameSession)
                    .ThenInclude(gs => gs.GameVariant)
                        .ThenInclude(gv => gv.Game)
                .Include(r => r.GameSession)
                    .ThenInclude(gs => gs.GameTable)
                        .ThenInclude(gt => gt.Hall)
                .Include(r => r.ReservationStatus)
                .Include(r => r.ReservationSeats)
                    .ThenInclude(rs => rs.Seat)
                .Include(r => r.Payments)
                    .ThenInclude(p => p.PaymentMethod)
                .Include(r => r.Payments)
                    .ThenInclude(p => p.PaymentStatus)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Player)
                .Include(r => r.GameSession)
                .Include(r => r.ReservationStatus)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            await LoadViewBagData();

            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reservation reservation)
        {
            if (id != reservation.ReservationId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadViewBagData();
                return View(reservation);
            }

            try
            {
                var existingReservation = await _context.Reservations.FindAsync(id);
                if (existingReservation == null)
                {
                    return NotFound();
                }

                existingReservation.PlayerId = reservation.PlayerId;
                existingReservation.GameSessionId = reservation.GameSessionId;
                existingReservation.ReservationStatusId = reservation.ReservationStatusId;
                existingReservation.ReservedAt = reservation.ReservedAt;

                _context.Update(existingReservation);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Rezerwacja została zaktualizowana.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(reservation.ReservationId))
                {
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError("", "Wystąpił błąd konfliktu podczas zapisywania. Spróbuj ponownie.");
                    await LoadViewBagData();
                    return View(reservation);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Wystąpił błąd: {ex.Message}");
                await LoadViewBagData();
                return View(reservation);
            }
        }

        private async Task LoadViewBagData()
        {
            ViewBag.Players = await _context.Players.ToListAsync();
            ViewBag.GameSessions = await _context.GameSessions
                .Include(gs => gs.GameVariant)
                    .ThenInclude(gv => gv.Game)
                .ToListAsync();
            ViewBag.ReservationStatuses = await _context.ReservationStatusDictionaries.ToListAsync();
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.ReservationId == id);
        }
    }
}
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
                .Include(r => r.Payments)
                    .ThenInclude(p => p.PaymentStatus)
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
                    .ThenInclude(gs => gs.GameVariant)
                        .ThenInclude(gv => gv.Game)
                .Include(r => r.ReservationStatus)
                .Include(r => r.Payments)
                    .ThenInclude(p => p.PaymentStatus)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            await LoadViewBagData();
            ViewBag.CurrentPaymentStatusId = GetLatestPaymentStatusId(reservation);

            return View(reservation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reservation reservation, int? paymentStatusId)
        {
            if (id != reservation.ReservationId)
            {
                return NotFound();
            }

            paymentStatusId ??= ReadPaymentStatusIdFromForm();

            ModelState.Remove("Player");
            ModelState.Remove("GameSession");
            ModelState.Remove("ReservationStatus");
            ModelState.Remove("ReservationSeats");
            ModelState.Remove("Payments");

            if (!ModelState.IsValid)
            {
                await LoadViewBagData();
                ViewBag.CurrentPaymentStatusId = paymentStatusId;
                return View(reservation);
            }

            try
            {
                var existingReservation = await _context.Reservations
                    .Include(r => r.Payments)
                    .FirstOrDefaultAsync(r => r.ReservationId == id);

                if (existingReservation == null)
                {
                    return NotFound();
                }

                existingReservation.PlayerId = reservation.PlayerId;
                existingReservation.GameSessionId = reservation.GameSessionId;
                existingReservation.ReservationStatusId = reservation.ReservationStatusId;
                existingReservation.ReservedAt = reservation.ReservedAt;

                if (paymentStatusId.HasValue)
                {
                    var result = await UpdatePaymentStatusForReservation(existingReservation, paymentStatusId.Value);
                    if (!result.Success)
                    {
                        ModelState.AddModelError("", result.Message);
                        await LoadViewBagData();
                        ViewBag.CurrentPaymentStatusId = paymentStatusId;
                        return View(reservation);
                    }
                }
                else
                {
                    TempData["Error"] = "Nie przesłano statusu płatności z formularza. Sprawdź pole Status płatności w widoku Edit.cshtml.";
                }

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

                ModelState.AddModelError("", "Wystąpił błąd konfliktu podczas zapisywania. Spróbuj ponownie.");
                await LoadViewBagData();
                ViewBag.CurrentPaymentStatusId = paymentStatusId;
                return View(reservation);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Wystąpił błąd: {ex.Message}");
                await LoadViewBagData();
                ViewBag.CurrentPaymentStatusId = paymentStatusId;
                return View(reservation);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Payments)
                    .ThenInclude(p => p.PaymentTransactions)
                .Include(r => r.ReservationSeats)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            try
            {
                foreach (var payment in reservation.Payments)
                {
                    if (payment.PaymentTransactions != null && payment.PaymentTransactions.Any())
                    {
                        _context.PaymentTransactions.RemoveRange(payment.PaymentTransactions);
                    }
                }

                if (reservation.Payments.Any())
                {
                    _context.Payments.RemoveRange(reservation.Payments);
                }

                if (reservation.ReservationSeats.Any())
                {
                    _context.ReservationSeats.RemoveRange(reservation.ReservationSeats);
                }

                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Rezerwacja została usunięta.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Błąd podczas usuwania: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var confirmedStatus = await _context.ReservationStatusDictionaries
                .FirstOrDefaultAsync(s => s.Name == "Confirmed");

            if (confirmedStatus == null)
            {
                TempData["Error"] = "Brak statusu rezerwacji Confirmed w bazie.";
                return RedirectToAction(nameof(Index));
            }

            reservation.ReservationStatusId = confirmedStatus.ReservationStatusId;

            var paidStatus = await _context.PaymentStatuses
                .FirstOrDefaultAsync(s => s.Name == "Paid");

            if (paidStatus != null && reservation.Payments.Any())
            {
                await UpdatePaymentStatusForReservation(reservation, paidStatus.PaymentStatusId);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezerwacja została zatwierdzona, a płatność oznaczona jako Paid.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            var cancelledStatus = await _context.ReservationStatusDictionaries
                .FirstOrDefaultAsync(s => s.Name == "Cancelled");

            if (cancelledStatus == null)
            {
                cancelledStatus = new ReservationStatusDictionary
                {
                    Name = "Cancelled",
                    Description = "Anulowana"
                };

                _context.ReservationStatusDictionaries.Add(cancelledStatus);
                await _context.SaveChangesAsync();
            }

            reservation.ReservationStatusId = cancelledStatus.ReservationStatusId;

            var rejectedStatus = await _context.PaymentStatuses
                .FirstOrDefaultAsync(s => s.Name == "Rejected");

            if (rejectedStatus != null && reservation.Payments.Any())
            {
                await UpdatePaymentStatusForReservation(reservation, rejectedStatus.PaymentStatusId);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezerwacja została anulowana.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadViewBagData()
        {
            ViewBag.Players = await _context.Players.ToListAsync();

            ViewBag.GameSessions = await _context.GameSessions
                .Include(gs => gs.GameVariant)
                    .ThenInclude(gv => gv.Game)
                .ToListAsync();

            ViewBag.ReservationStatuses = await _context.ReservationStatusDictionaries.ToListAsync();
            ViewBag.PaymentStatuses = await _context.PaymentStatuses.ToListAsync();
        }

        private static int? GetLatestPaymentStatusId(Reservation reservation)
        {
            return reservation.Payments
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault()
                ?.PaymentStatusId;
        }

        private int? ReadPaymentStatusIdFromForm()
        {
            if (Request.Form.TryGetValue("PaymentStatusId", out var valueFromCapitalName)
                && int.TryParse(valueFromCapitalName.FirstOrDefault(), out var parsedCapitalName))
            {
                return parsedCapitalName;
            }

            if (Request.Form.TryGetValue("paymentStatusId", out var valueFromCamelName)
                && int.TryParse(valueFromCamelName.FirstOrDefault(), out var parsedCamelName))
            {
                return parsedCamelName;
            }

            return null;
        }

        private async Task<(bool Success, string Message)> UpdatePaymentStatusForReservation(Reservation reservation, int paymentStatusId)
        {
            var selectedPaymentStatus = await _context.PaymentStatuses
                .FirstOrDefaultAsync(s => s.PaymentStatusId == paymentStatusId);

            if (selectedPaymentStatus == null)
            {
                return (false, "Wybrany status płatności nie istnieje.");
            }

            if (reservation.Payments == null || !reservation.Payments.Any())
            {
                return (false, "Ta rezerwacja nie ma powiązanej płatności w tabeli Payments.");
            }

            foreach (var payment in reservation.Payments)
            {
                payment.PaymentStatusId = selectedPaymentStatus.PaymentStatusId;

                if (selectedPaymentStatus.Name.Equals("Paid", StringComparison.OrdinalIgnoreCase))
                {
                    payment.PaidAt ??= DateTime.Now;
                }
                else
                {
                    payment.PaidAt = null;
                }
            }

            return (true, "OK");
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.ReservationId == id);
        }
    }
}

using GamblingBuddies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        
        private readonly AppDbContext _context;

        public ReservationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetGamesByHall(int hallId)
        {
            var games = _context.Set<GameSession>()
                .Where(s => s.GameTable.HallId == hallId)
                .Select(s => new
                {
                    gameId = s.GameVariant.Game.GameId,
                    name = s.GameVariant.Game.Name
                })
                .Distinct()
                .ToList();

            return Json(games);
        }

        [HttpGet]
        public IActionResult GetSessionsByHallAndGame(int hallId, int gameId)
        {
            var sessions = _context.Set<GameSession>()
                .Where(s => s.GameTable.HallId == hallId)
                .Where(s => s.GameVariant.Game.GameId == gameId)
                .Select(s => new
                {
                    gameSessionId = s.GameSessionId,

                    availableSeats =
                        s.GameTable.Seats.Count(seat => seat.IsActive)
                        -
                        s.Reservations
                            .SelectMany(r => r.ReservationSeats)
                            .Count(),

                    text = s.GameVariant.Name + " | " +
                           s.StartAt.ToString("dd.MM.yyyy HH:mm") + " - " +
                           s.EndAt.ToString("HH:mm")
                })
                .ToList()
                .Select(s => new
                {
                    s.gameSessionId,
                    s.availableSeats,
                    text = s.text + " | wolne miejsca: " + s.availableSeats
                })
                .ToList();

            return Json(sessions);
        }
        [HttpGet]
        public IActionResult Go()
        {
            ViewBag.Halls = _context.Set<Hall>()
                .Where(h => h.IsActive)
                .ToList();

            return View("Reservation");
        }

        [HttpPost]
        public IActionResult Go(int gameSessionId, int quantity)
        {
            var status = _context.Set<ReservationStatusDictionary>()
                .FirstOrDefault(s => s.Name == "Confirmed");

            var session = _context.Set<GameSession>()
                .FirstOrDefault(s => s.GameSessionId == gameSessionId);

            if (session == null || status == null)
            {
                return RedirectToAction("Go");
            }

            var freeSeats = _context.Set<Seat>()
                .Where(seat => seat.TableId == session.GameTableId)
                .Where(seat => seat.IsActive)
                .Where(seat => !_context.Set<ReservationSeat>()
                    .Any(rs => rs.SeatId == seat.SeatId && rs.GameSessionId == gameSessionId))
                .Take(quantity)
                .ToList();

            if (freeSeats.Count < quantity)
            {
                return RedirectToAction("Go");
            }

            var reservation = new Reservation
            {
                PlayerId = 1,
                GameSessionId = gameSessionId,
                ReservationStatusId = status.ReservationStatusId,
                ReservedAt = DateTime.Now
            };

            _context.Set<Reservation>().Add(reservation);
            _context.SaveChanges();

            foreach (var seat in freeSeats)
            {
                _context.Set<ReservationSeat>().Add(new ReservationSeat
                {
                    ReservationId = reservation.ReservationId,
                    SeatId = seat.SeatId,
                    GameSessionId = gameSessionId
                });
            }

            var pendingPaymentStatus = _context.Set<PaymentStatusDictionary>()
    .FirstOrDefault(p => p.Name == "Pending");

            var cashMethod = _context.Set<PaymentMethodDictionary>()
                .FirstOrDefault(p => p.Name == "Cash");

            if (pendingPaymentStatus != null && cashMethod != null)
            {
                var payment = new Payment
                {
                    ReservationId = reservation.ReservationId,
                    PaymentMethodId = cashMethod.PaymentMethodId,
                    PaymentStatusId = pendingPaymentStatus.PaymentStatusId,
                    Amount = 0,
                    CreatedAt = DateTime.Now,
                    PaidAt = null
                };

                _context.Set<Payment>().Add(payment);
            }

            _context.SaveChanges();

            TempData["Success"] = "Rezerwacja została utworzona.";

            return RedirectToAction("Go");
        }
    }
}

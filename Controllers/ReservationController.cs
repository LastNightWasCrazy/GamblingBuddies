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
        public IActionResult Go()
        {
            ViewBag.Halls = _context.Set<Hall>()
                .Where(h => h.IsActive)
                .OrderBy(h => h.Name)
                .ToList();

            return View("Reservation");
        }

        [HttpGet]
        public IActionResult GetGamesByHall(int hallId)
        {
            var games = (
                from session in _context.GameSessions
                join table in _context.GameTables
                    on session.GameTableId equals table.GameTableId
                join variant in _context.GameVariants
                    on session.GameVariantId equals variant.GameVariantId
                join game in _context.Games
                    on variant.GameId equals game.GameId
                where table.HallId == hallId
                      && table.IsActive
                      && game.IsActive
                select new
                {
                    gameId = game.GameId,
                    name = game.Name
                }
            )
            .Distinct()
            .OrderBy(g => g.name)
            .ToList();

            return Json(games);
        }

        [HttpGet]
        public IActionResult GetTablesByHallAndGame(int hallId, int gameId)
        {
            var tables = (
                from table in _context.GameTables
                join session in _context.GameSessions
                    on table.GameTableId equals session.GameTableId
                join variant in _context.GameVariants
                    on session.GameVariantId equals variant.GameVariantId
                join game in _context.Games
                    on variant.GameId equals game.GameId
                where table.HallId == hallId
                      && game.GameId == gameId
                      && table.IsActive
                select new
                {
                    gameTableId = table.GameTableId,
                    text = "Stół " + table.TableNumber + " | max graczy: " + table.MaxPlayers,
                    maxPlayers = table.MaxPlayers
                }
            )
            .Distinct()
            .OrderBy(t => t.text)
            .ToList();

            return Json(tables);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Go(int HallId, int GameId, int GameTableId, DateTime ReservationDate, string ReservationTime, int Quantity, string PaymentOption)
        {
            if (HallId <= 0 || GameId <= 0 || GameTableId <= 0)
            {
                TempData["Error"] = "Wybierz salę, grę oraz stół.";
                return RedirectToAction("Go");
            }

            if (Quantity <= 0)
            {
                TempData["Error"] = "Liczba miejsc musi być większa od zera.";
                return RedirectToAction("Go");
            }

            if (string.IsNullOrWhiteSpace(ReservationTime) || !TimeSpan.TryParse(ReservationTime, out var time))
            {
                TempData["Error"] = "Wybierz poprawną godzinę.";
                return RedirectToAction("Go");
            }

            var startAt = ReservationDate.Date.Add(time);
            var endAt = startAt.AddHours(3);

            if (startAt <= DateTime.Now)
            {
                TempData["Error"] = "Nie można utworzyć rezerwacji w przeszłości.";
                return RedirectToAction("Go");
            }

            var table = _context.GameTables
                .Include(t => t.Seats)
                .FirstOrDefault(t => t.GameTableId == GameTableId && t.HallId == HallId && t.IsActive);

            if (table == null)
            {
                TempData["Error"] = "Wybrany stół nie istnieje albo jest nieaktywny.";
                return RedirectToAction("Go");
            }

            var variant = _context.GameVariants
                .FirstOrDefault(v => v.GameId == GameId && v.IsActive);

            if (variant == null)
            {
                TempData["Error"] = "Brak aktywnego wariantu dla wybranej gry.";
                return RedirectToAction("Go");
            }

            if (Quantity > table.MaxPlayers)
            {
                TempData["Error"] = $"Nie można zarezerwować więcej niż {table.MaxPlayers} miejsc przy tym stole.";
                return RedirectToAction("Go");
            }

            var activeSeatsCount = table.Seats.Count(s => s.IsActive);

            if (Quantity > activeSeatsCount)
            {
                TempData["Error"] = $"Przy stole zdefiniowano tylko {activeSeatsCount} aktywnych miejsc.";
                return RedirectToAction("Go");
            }

            var plannedStatus = _context.SessionStatusDictionaries
                .FirstOrDefault(s => s.Name == "Planned");

            if (plannedStatus == null)
            {
                TempData["Error"] = "Brak statusu sesji Planned w bazie.";
                return RedirectToAction("Go");
            }

            var pendingStatus = _context.ReservationStatusDictionaries
                .FirstOrDefault(s => s.Name == "Pending");

            if (pendingStatus == null)
            {
                TempData["Error"] = "Brak statusu rezerwacji Pending w bazie.";
                return RedirectToAction("Go");
            }

            var overlappingSession = _context.GameSessions
                .Any(s =>
                    s.GameTableId == GameTableId &&
                    s.StartAt < endAt &&
                    s.EndAt > startAt);

            if (overlappingSession)
            {
                TempData["Error"] = "W wybranym terminie ten stół ma już zaplanowaną sesję.";
                return RedirectToAction("Go");
            }

            var session = new GameSession
            {
                GameVariantId = variant.GameVariantId,
                GameTableId = GameTableId,
                StartAt = startAt,
                EndAt = endAt,
                SessionStatusId = plannedStatus.SessionStatusId,
                CreatedByUserId = GetCurrentUserId()
            };

            _context.GameSessions.Add(session);
            _context.SaveChanges();

            var player = GetOrCreatePlayerForCurrentUser();

            var reservation = new Reservation
            {
                PlayerId = player.PlayerId,
                GameSessionId = session.GameSessionId,
                ReservationStatusId = pendingStatus.ReservationStatusId,
                ReservedAt = DateTime.Now
            };

            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            var freeSeats = table.Seats
                .Where(s => s.IsActive)
                .Take(Quantity)
                .ToList();

            foreach (var seat in freeSeats)
            {
                _context.ReservationSeats.Add(new ReservationSeat
                {
                    ReservationId = reservation.ReservationId,
                    SeatId = seat.SeatId,
                    GameSessionId = session.GameSessionId
                });
            }

            _context.SaveChanges();

            TempData["Success"] = "Rezerwacja została utworzona.";
            TempData["ReservationId"] = reservation.ReservationId;

            var pendingPaymentStatus = _context.PaymentStatuses
     .FirstOrDefault(s => s.Name == "Pending");

            if (pendingPaymentStatus == null)
            {
                TempData["Error"] = "Brak statusu płatności Pending w bazie.";
                return RedirectToAction("Go");
            }

            var paymentMethod = _context.PaymentMethods
                .FirstOrDefault(m => m.Name == PaymentOption);

            if (paymentMethod == null)
            {
                TempData["Error"] = $"Brak metody płatności {PaymentOption} w bazie.";
                return RedirectToAction("Go");
            }

            var payment = new Payment
            {
                ReservationId = reservation.ReservationId,
                PaymentMethodId = paymentMethod.PaymentMethodId,
                PaymentStatusId = pendingPaymentStatus.PaymentStatusId,
                Amount = Quantity * 50,
                CreatedAt = DateTime.Now
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            if (PaymentOption == "Card")
            {
                return RedirectToAction("Pay", "Payment", new { reservationId = reservation.ReservationId });
            }

            TempData["Success"] = "Rezerwacja została utworzona. Płatność gotówką będzie wykonana na miejscu.";

            return RedirectToAction("Go");
        }

        private int GetCurrentUserId()
        {
            var login = User.Identity?.Name;

            var user = _context.SystemUsers
                .FirstOrDefault(u => u.Login == login);

            if (user != null)
                return user.SystemUserId;

            return _context.SystemUsers
                .OrderBy(u => u.SystemUserId)
                .Select(u => u.SystemUserId)
                .First();
        }

        private Player GetOrCreatePlayerForCurrentUser()
        {
            var login = User.Identity?.Name ?? "user";
            var email = $"{login}@local.test";

            var player = _context.Players
                .FirstOrDefault(p => p.Email == email);

            if (player != null)
                return player;

            player = new Player
            {
                FirstName = "Użytkownik",
                LastName = login,
                Email = email,
                Phone = "000000000",
                CreatedAt = DateTime.Now
            };

            _context.Players.Add(player);
            _context.SaveChanges();

            return player;
        }
    }
}
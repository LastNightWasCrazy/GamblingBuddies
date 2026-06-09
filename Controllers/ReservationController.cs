using GamblingBuddies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GamblingBuddies.Controllers
{
    public class ReservationController : Controller
    {
        private readonly AppDbContext _context;

        public ReservationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Go()
        {
            ViewBag.Halls = _context.Set<Hall>()
                .Where(h => h.IsActive)
                .OrderBy(h => h.Name)
                .ToList();

            return View("Reservation");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetGamesByHall(int hallId)
        {
            var games = _context.GameTableGames
                .Include(gtg => gtg.Game)
                .Include(gtg => gtg.GameTable)
                .Where(gtg =>
                    gtg.GameTable.HallId == hallId &&
                    gtg.GameTable.IsActive &&
                    gtg.Game.IsActive)
                .Select(gtg => new
                {
                    gameId = gtg.Game.GameId,
                    name = gtg.Game.Name
                })
                .Distinct()
                .OrderBy(g => g.name)
                .ToList();

            return Json(games);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetTablesByHallAndGame(int hallId, int gameId)
        {
            var tables = _context.GameTableGames
                .Include(gtg => gtg.GameTable)
                .Where(gtg =>
                    gtg.GameId == gameId &&
                    gtg.GameTable.HallId == hallId &&
                    gtg.GameTable.IsActive)
                .Select(gtg => new
                {
                    gameTableId = gtg.GameTable.GameTableId,
                    text = "Stół " + gtg.GameTable.TableNumber + " | maks. osób: " + gtg.GameTable.MaxPlayers,
                    maxPlayers = gtg.GameTable.MaxPlayers
                })
                .Distinct()
                .OrderBy(t => t.text)
                .ToList();

            return Json(tables);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CheckTableAvailability(
            int gameTableId,
            string reservationDate,
            string reservationTime,
            int durationHours)
        {
            if (gameTableId <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Nie wybrano stołu."
                });
            }

            if (string.IsNullOrWhiteSpace(reservationDate) ||
                !DateTime.TryParse(reservationDate, out var date))
            {
                return Json(new
                {
                    success = false,
                    message = "Nie wybrano poprawnej daty."
                });
            }

            if (string.IsNullOrWhiteSpace(reservationTime) ||
                !TimeSpan.TryParse(reservationTime, out var time))
            {
                return Json(new
                {
                    success = false,
                    message = "Nie wybrano poprawnej godziny."
                });
            }

            if (durationHours < 1 || durationHours > 4)
            {
                return Json(new
                {
                    success = false,
                    message = "Czas rezerwacji musi wynosić od 1 do 4 godzin."
                });
            }

            var startAt = date.Date.Add(time);
            var endAt = startAt.AddHours(durationHours);

            if (startAt <= DateTime.Now)
            {
                return Json(new
                {
                    success = false,
                    message = "Nie można rezerwować stołu w przeszłości."
                });
            }

            var table = _context.GameTables
                .FirstOrDefault(t =>
                    t.GameTableId == gameTableId &&
                    t.IsActive);

            if (table == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Wybrany stół nie istnieje albo jest nieaktywny."
                });
            }

            var isTableReserved = _context.GameSessions
                .Any(s =>
                    s.GameTableId == gameTableId &&
                    s.StartAt < endAt &&
                    s.EndAt > startAt);

            if (isTableReserved)
            {
                return Json(new
                {
                    success = false,
                    message = "Ten stół jest już zarezerwowany w wybranym terminie."
                });
            }

            return Json(new
            {
                success = true,
                maxPlayers = table.MaxPlayers,
                message = $"Stół jest dostępny od {startAt:HH:mm} do {endAt:HH:mm}."
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Go(
            int HallId,
            int GameId,
            int GameTableId,
            DateTime ReservationDate,
            string ReservationTime,
            int DurationHours,
            int Quantity,
            string PaymentOption,
            string PlayerFirstName,
            string PlayerLastName,
            string PlayerEmail,
            string? PlayerPhone)
        {
            if (string.IsNullOrWhiteSpace(PlayerFirstName) ||
                string.IsNullOrWhiteSpace(PlayerLastName) ||
                string.IsNullOrWhiteSpace(PlayerEmail))
            {
                TempData["Error"] = "Podaj imię, nazwisko oraz email.";
                return RedirectToAction("Go");
            }

            if (!IsValidEmail(PlayerEmail))
            {
                TempData["Error"] = "Podaj poprawny adres email.";
                return RedirectToAction("Go");
            }

            if (!string.IsNullOrWhiteSpace(PlayerPhone) && !IsValidPhone(PlayerPhone))
            {
                TempData["Error"] = "Podaj poprawny numer telefonu. Numer powinien mieć od 9 do 15 cyfr.";
                return RedirectToAction("Go");
            }

            if (HallId <= 0 || GameId <= 0 || GameTableId <= 0)
            {
                TempData["Error"] = "Wybierz salę, grę oraz stół.";
                return RedirectToAction("Go");
            }

            if (Quantity <= 0)
            {
                TempData["Error"] = "Liczba osób musi być większa od zera.";
                return RedirectToAction("Go");
            }

            if (DurationHours < 1 || DurationHours > 4)
            {
                TempData["Error"] = "Czas rezerwacji musi wynosić od 1 do 4 godzin.";
                return RedirectToAction("Go");
            }

            if (string.IsNullOrWhiteSpace(ReservationTime) ||
                !TimeSpan.TryParse(ReservationTime, out var time))
            {
                TempData["Error"] = "Wybierz poprawną godzinę.";
                return RedirectToAction("Go");
            }

            var startAt = ReservationDate.Date.Add(time);
            var endAt = startAt.AddHours(DurationHours);

            if (startAt <= DateTime.Now)
            {
                TempData["Error"] = "Nie można utworzyć rezerwacji w przeszłości.";
                return RedirectToAction("Go");
            }

            var table = _context.GameTables
                .Include(t => t.Seats)
                .Include(t => t.GameTableGames)
                .FirstOrDefault(t =>
                    t.GameTableId == GameTableId &&
                    t.HallId == HallId &&
                    t.IsActive);

            if (table == null)
            {
                TempData["Error"] = "Wybrany stół nie istnieje albo jest nieaktywny.";
                return RedirectToAction("Go");
            }

            var tableSupportsGame = table.GameTableGames
                .Any(gtg => gtg.GameId == GameId);

            if (!tableSupportsGame)
            {
                TempData["Error"] = "Wybrany stół nie obsługuje tej gry.";
                return RedirectToAction("Go");
            }

            if (Quantity > table.MaxPlayers)
            {
                TempData["Error"] = $"Ten stół obsługuje maksymalnie {table.MaxPlayers} osób.";
                return RedirectToAction("Go");
            }

            var variant = _context.GameVariants
                .FirstOrDefault(v =>
                    v.GameId == GameId &&
                    v.IsActive);

            if (variant == null)
            {
                TempData["Error"] = "Brak aktywnego wariantu dla wybranej gry.";
                return RedirectToAction("Go");
            }

            var plannedStatus = _context.SessionStatusDictionaries
                .FirstOrDefault(s => s.Name == "Planned");

            if (plannedStatus == null)
            {
                TempData["Error"] = "Brak statusu sesji Planned w bazie.";
                return RedirectToAction("Go");
            }

            var pendingReservationStatus = _context.ReservationStatusDictionaries
                .FirstOrDefault(s => s.Name == "Pending");

            if (pendingReservationStatus == null)
            {
                TempData["Error"] = "Brak statusu rezerwacji Pending w bazie.";
                return RedirectToAction("Go");
            }

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

            var isTableReserved = _context.GameSessions
                .Any(s =>
                    s.GameTableId == GameTableId &&
                    s.StartAt < endAt &&
                    s.EndAt > startAt);

            if (isTableReserved)
            {
                TempData["Error"] = "Ten stół jest już zarezerwowany w wybranym terminie. Wybierz inny stół albo inną godzinę.";
                return RedirectToAction("Go");
            }

            var session = new GameSession
            {
                GameVariantId = variant.GameVariantId,
                GameTableId = GameTableId,
                StartAt = startAt,
                EndAt = endAt,
                SessionStatusId = plannedStatus.SessionStatusId,
                CreatedByUserId = GetCurrentUserIdOrDefault()
            };

            _context.GameSessions.Add(session);
            _context.SaveChanges();

            var player = GetOrCreatePlayer(
                PlayerFirstName,
                PlayerLastName,
                PlayerEmail,
                PlayerPhone);

            var reservation = new Reservation
            {
                PlayerId = player.PlayerId,
                GameSessionId = session.GameSessionId,
                ReservationStatusId = pendingReservationStatus.ReservationStatusId,
                ReservedAt = DateTime.Now
            };

            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            var seatsForReservation = table.Seats
                .Where(s => s.IsActive)
                .OrderBy(s => s.SeatNumber)
                .Take(Quantity)
                .ToList();

            foreach (var seat in seatsForReservation)
            {
                _context.ReservationSeats.Add(new ReservationSeat
                {
                    ReservationId = reservation.ReservationId,
                    SeatId = seat.SeatId,
                    GameSessionId = session.GameSessionId
                });
            }

            var payment = new Payment
            {
                ReservationId = reservation.ReservationId,
                PaymentMethodId = paymentMethod.PaymentMethodId,
                PaymentStatusId = pendingPaymentStatus.PaymentStatusId,
                Amount = Quantity * DurationHours * 50,
                CreatedAt = DateTime.Now
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            TempData["ReservationId"] = reservation.ReservationId;

            if (PaymentOption == "Card")
            {
                return RedirectToAction("Pay", "Payment", new { reservationId = reservation.ReservationId });
            }

            TempData["Success"] = $"Rezerwacja całego stołu została utworzona od {startAt:HH:mm} do {endAt:HH:mm}. Płatność gotówką będzie wykonana na miejscu.";

            return RedirectToAction("Go");
        }

        private Player GetOrCreatePlayer(
            string firstName,
            string lastName,
            string email,
            string? phone)
        {
            email = email.Trim().ToLower();

            var player = _context.Players
                .FirstOrDefault(p => p.Email.ToLower() == email);

            if (player != null)
            {
                player.FirstName = firstName.Trim();
                player.LastName = lastName.Trim();

                if (!string.IsNullOrWhiteSpace(phone))
                {
                    player.Phone = phone.Trim();
                }

                _context.SaveChanges();

                return player;
            }

            player = new Player
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email,
                Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.Players.Add(player);
            _context.SaveChanges();

            return player;
        }

        private int GetCurrentUserIdOrDefault()
        {
            var login = User.Identity?.Name;

            if (!string.IsNullOrWhiteSpace(login))
            {
                var user = _context.SystemUsers
                    .FirstOrDefault(u => u.Login == login);

                if (user != null)
                    return user.SystemUserId;
            }

            var admin = _context.SystemUsers
                .FirstOrDefault(u => u.Login == "admin");

            if (admin != null)
                return admin.SystemUserId;

            return _context.SystemUsers
                .OrderBy(u => u.SystemUserId)
                .Select(u => u.SystemUserId)
                .First();
        }

        private bool IsValidEmail(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }

        private bool IsValidPhone(string phone)
        {
            phone = phone.Replace(" ", "").Replace("-", "");

            return Regex.IsMatch(phone, @"^\+?[0-9]{9,15}$");
        }
    }
}
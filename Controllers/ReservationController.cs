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
            ViewBag.Halls = _context.Halls
                .Where(h => h.IsActive)
                .OrderBy(h => h.Name)
                .ToList();

            return View("Reservation");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Go(int HallId, int GameId, int GameSessionId, string ReservationType, int Quantity)
        {
            if (GameSessionId <= 0)
            {
                TempData["ErrorMessage"] = "Wybierz termin rezerwacji.";
                return RedirectToAction("Go");
            }

            var player = _context.Players.FirstOrDefault();

            if (player == null)
            {
                player = new Player
                {
                    FirstName = "Gość",
                    LastName = User.Identity?.Name ?? "Użytkownik",
                    Email = $"{User.Identity?.Name ?? "user"}@local.test",
                    Phone = "000000000",
                    CreatedAt = DateTime.Now
                };

                _context.Players.Add(player);
                _context.SaveChanges();
            }

            var status = _context.ReservationStatusDictionaries
                .FirstOrDefault(s => s.Name == "Pending")
                ?? _context.ReservationStatusDictionaries.FirstOrDefault();

            if (status == null)
            {
                TempData["ErrorMessage"] = "Brak statusów rezerwacji w bazie.";
                return RedirectToAction("Go");
            }

            var reservation = new Reservation
            {
                PlayerId = player.PlayerId,
                GameSessionId = GameSessionId,
                ReservationStatusId = status.ReservationStatusId,
                ReservedAt = DateTime.Now
            };

            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Rezerwacja została utworzona pomyślnie.";

            return RedirectToAction("Go");
        }

        [HttpGet]
        public IActionResult GetGamesByHall(int hallId)
        {
            var games = _context.GameSessions
                .Include(s => s.GameTable)
                .Include(s => s.GameVariant)
                    .ThenInclude(v => v.Game)
                .Where(s => s.GameTable.HallId == hallId)
                .Where(s => s.GameVariant.Game.IsActive)
                .Select(s => new
                {
                    gameId = s.GameVariant.Game.GameId,
                    name = s.GameVariant.Game.Name
                })
                .Distinct()
                .OrderBy(g => g.name)
                .ToList();

            return Json(games);
        }

        [HttpGet]
        public IActionResult GetSessionsByHallAndGame(int hallId, int gameId)
        {
            var sessions = (
                from session in _context.GameSessions
                join table in _context.GameTables
                    on session.GameTableId equals table.GameTableId
                join variant in _context.GameVariants
                    on session.GameVariantId equals variant.GameVariantId
                join game in _context.Games
                    on variant.GameId equals game.GameId
                where table.HallId == hallId
                      && game.GameId == gameId
                      && session.EndAt > DateTime.Now
                orderby session.StartAt
                select new
                {
                    gameSessionId = session.GameSessionId,
                    text = variant.Name + " | " +
                           session.StartAt.ToString("dd.MM.yyyy HH:mm") + " - " +
                           session.EndAt.ToString("HH:mm")
                }
            ).ToList();

            return Json(sessions);
        }
    }
}
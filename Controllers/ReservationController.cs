using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                    text = s.GameVariant.Name + " | " +
                           s.StartAt.ToString("dd.MM.yyyy HH:mm") + " - " +
                           s.EndAt.ToString("HH:mm")
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
    }
}

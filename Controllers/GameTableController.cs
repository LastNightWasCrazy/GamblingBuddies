using GamblingBuddies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Controllers
{
    [Authorize(Roles = "Administrator,Manager")]
    public class GameTablesController : Controller
    {
        private readonly AppDbContext _context;

        public GameTablesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var tables = _context.GameTables
                .Include(t => t.Hall)
                .ToList();

            return View(tables);
        }

        public IActionResult Details(int id)
        {
            var table = _context.GameTables
                .Include(t => t.Hall)
                .Include(t => t.Seats)
                .Include(t => t.GameSessions)
                    .ThenInclude(gs => gs.GameVariant)
                        .ThenInclude(gv => gv.Game)
                .Include(t => t.GameSessions)
                    .ThenInclude(gs => gs.SessionStatus)
                .Include(t => t.GameSessions)
                    .ThenInclude(gs => gs.Reservations)
                        .ThenInclude(r => r.ReservationStatus)
                .Include(t => t.GameSessions)
                    .ThenInclude(gs => gs.Reservations)
                        .ThenInclude(r => r.ReservationSeats)
                .FirstOrDefault(t => t.GameTableId == id);

            if (table == null)
                return NotFound();

            return View(table);
        }
    }
}
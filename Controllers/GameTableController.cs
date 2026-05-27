using GamblingBuddies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var table = _context.GameTables.FirstOrDefault(t => t.GameTableId == id);

            if (table == null)
                return NotFound();

            ViewBag.Halls = new SelectList(
                _context.Halls.Where(h => h.IsActive).OrderBy(h => h.Name).ToList(),
                "HallId",
                "Name",
                table.HallId
            );

            return View(table);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(GameTable model)
        {
            var table = _context.GameTables.FirstOrDefault(t => t.GameTableId == model.GameTableId);

            if (table == null)
                return NotFound();

            if (model.MinPlayers < 1)
            {
                ModelState.AddModelError("MinPlayers", "Minimalna liczba graczy musi być większa od 0.");
            }

            if (model.MaxPlayers < model.MinPlayers)
            {
                ModelState.AddModelError("MaxPlayers", "Maksymalna liczba graczy nie może być mniejsza niż minimalna.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Halls = new SelectList(
                    _context.Halls.Where(h => h.IsActive).OrderBy(h => h.Name).ToList(),
                    "HallId",
                    "Name",
                    model.HallId
                );

                return View(model);
            }

            table.HallId = model.HallId;
            table.TableNumber = model.TableNumber;
            table.MinPlayers = model.MinPlayers;
            table.MaxPlayers = model.MaxPlayers;
            table.IsActive = model.IsActive;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Stół został zaktualizowany.";

            return RedirectToAction("Index");
        }
    }
}
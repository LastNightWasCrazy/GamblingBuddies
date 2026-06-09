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
                .Include(t => t.GameTableGames)
                    .ThenInclude(gtg => gtg.Game)
                .OrderBy(t => t.Hall.Name)
                .ThenBy(t => t.TableNumber)
                .ToList();

            return View(tables);
        }

        public IActionResult Details(int id)
        {
            var table = _context.GameTables
                .Include(t => t.Hall)
                .Include(t => t.GameTableGames)
                    .ThenInclude(gtg => gtg.Game)
                .Include(t => t.GameSessions)
                    .ThenInclude(gs => gs.GameVariant)
                        .ThenInclude(gv => gv.Game)
                .Include(t => t.GameSessions)
                    .ThenInclude(gs => gs.SessionStatus)
                .FirstOrDefault(t => t.GameTableId == id);

            if (table == null)
                return NotFound();

            return View(table);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadHalls();
            LoadGames();

            var model = new GameTable
            {
                IsActive = true,
                MinPlayers = 1,
                MaxPlayers = 6
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(GameTable model, int[] selectedGameIds)
        {
            ModelState.Remove("Hall");
            ModelState.Remove("Seats");
            ModelState.Remove("GameSessions");
            ModelState.Remove("GameTableGames");

            ValidateGameTable(model, selectedGameIds);

            var tableNumberExists = _context.GameTables
                .Any(t => t.TableNumber == model.TableNumber && t.HallId == model.HallId);

            if (tableNumberExists)
            {
                ModelState.AddModelError("TableNumber", "W tej sali istnieje już stół o takim numerze.");
            }

            if (!ModelState.IsValid)
            {
                LoadHalls(model.HallId);
                LoadGames(selectedGameIds);
                return View(model);
            }

            _context.GameTables.Add(model);
            _context.SaveChanges();

            foreach (var gameId in selectedGameIds.Distinct())
            {
                _context.GameTableGames.Add(new GameTableGame
                {
                    GameTableId = model.GameTableId,
                    GameId = gameId
                });
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Stół został dodany razem z przypisanymi grami.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var table = _context.GameTables
                .Include(t => t.GameTableGames)
                .FirstOrDefault(t => t.GameTableId == id);

            if (table == null)
                return NotFound();

            var selectedGameIds = table.GameTableGames
                .Select(gtg => gtg.GameId)
                .ToArray();

            LoadHalls(table.HallId);
            LoadGames(selectedGameIds);

            return View(table);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(GameTable model, int[] selectedGameIds)
        {
            ModelState.Remove("Hall");
            ModelState.Remove("Seats");
            ModelState.Remove("GameSessions");
            ModelState.Remove("GameTableGames");

            var table = _context.GameTables
                .Include(t => t.GameTableGames)
                .FirstOrDefault(t => t.GameTableId == model.GameTableId);

            if (table == null)
                return NotFound();

            ValidateGameTable(model, selectedGameIds);

            var tableNumberExists = _context.GameTables
                .Any(t =>
                    t.GameTableId != model.GameTableId &&
                    t.TableNumber == model.TableNumber &&
                    t.HallId == model.HallId);

            if (tableNumberExists)
            {
                ModelState.AddModelError("TableNumber", "W tej sali istnieje już stół o takim numerze.");
            }

            if (!ModelState.IsValid)
            {
                LoadHalls(model.HallId);
                LoadGames(selectedGameIds);
                return View(model);
            }

            table.HallId = model.HallId;
            table.TableNumber = model.TableNumber;
            table.MinPlayers = model.MinPlayers;
            table.MaxPlayers = model.MaxPlayers;
            table.IsActive = model.IsActive;

            var oldAssignments = table.GameTableGames.ToList();
            _context.GameTableGames.RemoveRange(oldAssignments);

            foreach (var gameId in selectedGameIds.Distinct())
            {
                _context.GameTableGames.Add(new GameTableGame
                {
                    GameTableId = table.GameTableId,
                    GameId = gameId
                });
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Stół został zaktualizowany.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var table = _context.GameTables
                .Include(t => t.GameTableGames)
                .FirstOrDefault(t => t.GameTableId == id);

            if (table == null)
                return NotFound();

            var hasSessions = _context.GameSessions
                .Any(gs => gs.GameTableId == id);

            if (hasSessions)
            {
                table.IsActive = false;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Stół ma powiązane sesje, więc został tylko dezaktywowany.";
                return RedirectToAction(nameof(Index));
            }

            _context.GameTableGames.RemoveRange(table.GameTableGames);
            _context.GameTables.Remove(table);

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Stół został usunięty.";

            return RedirectToAction(nameof(Index));
        }

        private void ValidateGameTable(GameTable model, int[] selectedGameIds)
        {
            if (model.HallId <= 0)
            {
                ModelState.AddModelError("HallId", "Wybierz salę.");
            }

            if (model.TableNumber < 1)
            {
                ModelState.AddModelError("TableNumber", "Numer stołu musi być większy od 0.");
            }

            if (model.MinPlayers < 1)
            {
                ModelState.AddModelError("MinPlayers", "Minimalna liczba graczy musi być większa od 0.");
            }

            if (model.MaxPlayers < model.MinPlayers)
            {
                ModelState.AddModelError("MaxPlayers", "Maksymalna liczba graczy nie może być mniejsza niż minimalna.");
            }

            if (model.MaxPlayers > 20)
            {
                ModelState.AddModelError("MaxPlayers", "Maksymalna liczba graczy nie może być większa niż 20.");
            }

            if (selectedGameIds == null || selectedGameIds.Length == 0)
            {
                ModelState.AddModelError("", "Wybierz przynajmniej jedną grę dla stołu.");
            }
        }

        private void LoadHalls(int? selectedHallId = null)
        {
            ViewBag.Halls = new SelectList(
                _context.Halls
                    .Where(h => h.IsActive)
                    .OrderBy(h => h.Name)
                    .ToList(),
                "HallId",
                "Name",
                selectedHallId
            );
        }

        private void LoadGames(int[]? selectedGameIds = null)
        {
            selectedGameIds ??= Array.Empty<int>();

            ViewBag.Games = _context.Games
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name)
                .ToList();

            ViewBag.SelectedGameIds = selectedGameIds;
        }
    }
}
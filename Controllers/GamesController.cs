using GamblingBuddies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Controllers
{
    [Authorize(Roles = "Administrator,Manager")]
    public class GamesController : Controller
    {
        private readonly AppDbContext _context;

        public GamesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var games = _context.Games
                .Include(g => g.GameCategory)
                .ToList();

            return View(games);
        }

        public IActionResult Details(int id)
        {
            var game = _context.Games
                .Include(g => g.GameCategory)
                .Include(g => g.GameVariants)
                .FirstOrDefault(g => g.GameId == id);

            if (game == null)
                return NotFound();

            return View(game);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var game = _context.Games
                .FirstOrDefault(g => g.GameId == id);

            if (game == null)
                return NotFound();

            ViewBag.Categories = new SelectList(
                _context.GameCategoryDictionaries
                    .OrderBy(c => c.Name)
                    .ToList(),
                "GameCategoryId",
                "Name",
                game.GameCategoryId
            );

            return View(game);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Game model)
        {
            var game = _context.Games
                .FirstOrDefault(g => g.GameId == model.GameId);

            if (game == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Nazwa gry jest wymagana.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    _context.GameCategoryDictionaries
                        .OrderBy(c => c.Name)
                        .ToList(),
                    "GameCategoryId",
                    "Name",
                    model.GameCategoryId
                );

                return View(model);
            }

            game.Name = model.Name;
            game.Description = model.Description;
            game.GameCategoryId = model.GameCategoryId;
            game.IsActive = model.IsActive;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Gra została zaktualizowana.";

            return RedirectToAction("Index");
        }
    }
}
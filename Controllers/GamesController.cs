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
                .Include(g => g.GameVariants)
                .OrderBy(g => g.Name)
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
            {
                return NotFound();
            }

            return View(game);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadCategories();
            return View(new GameCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(GameCreateViewModel model)
        {
            if (model.DefaultMaxBet < model.DefaultMinBet)
            {
                ModelState.AddModelError("DefaultMaxBet", "Maksymalna stawka nie może być mniejsza od minimalnej.");
            }

            var gameExists = _context.Games.Any(g => g.Name == model.Name);
            if (gameExists)
            {
                ModelState.AddModelError("Name", "Gra o takiej nazwie już istnieje.");
            }

            var categoryExists = _context.GameCategoryDictionaries
                .Any(c => c.GameCategoryId == model.GameCategoryId);

            if (!categoryExists)
            {
                ModelState.AddModelError("GameCategoryId", "Wybrana kategoria nie istnieje.");
            }

            if (!ModelState.IsValid)
            {
                LoadCategories(model.GameCategoryId);
                return View(model);
            }

            var game = new Game
            {
                Name = model.Name.Trim(),
                GameCategoryId = model.GameCategoryId,
                Description = model.Description,
                IsActive = model.IsActive
            };

            _context.Games.Add(game);
            _context.SaveChanges();

            var variant = new GameVariant
            {
                GameId = game.GameId,
                Name = model.VariantName.Trim(),
                RulesDescription = model.RulesDescription.Trim(),
                DefaultMinBet = model.DefaultMinBet,
                DefaultMaxBet = model.DefaultMaxBet,
                IsActive = model.VariantIsActive
            };

            _context.GameVariants.Add(variant);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Gra i wariant zostały dodane.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var game = _context.Games
                .FirstOrDefault(g => g.GameId == id);

            if (game == null)
                return NotFound();

            LoadCategories(game.GameCategoryId);

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

            var duplicateName = _context.Games.Any(g =>
                g.GameId != model.GameId &&
                g.Name == model.Name);

            if (duplicateName)
            {
                ModelState.AddModelError("Name", "Gra o takiej nazwie już istnieje.");
            }

            if (!ModelState.IsValid)
            {
                LoadCategories(model.GameCategoryId);
                return View(model);
            }

            game.Name = model.Name.Trim();
            game.Description = model.Description;
            game.GameCategoryId = model.GameCategoryId;
            game.IsActive = model.IsActive;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Gra została zaktualizowana.";

            return RedirectToAction(nameof(Index));
        }

        private void LoadCategories(int? selectedCategoryId = null)
        {
            ViewBag.Categories = new SelectList(
                _context.GameCategoryDictionaries
                    .OrderBy(c => c.Name)
                    .ToList(),
                "GameCategoryId",
                "Name",
                selectedCategoryId
            );
        }
    }
}

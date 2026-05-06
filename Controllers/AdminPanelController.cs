using GamblingBuddies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Controllers
{
    [Authorize(Roles = "Administrator,Manager")]
    public class AdminPanelController : Controller
    {
        private readonly AppDbContext _context;

        public AdminPanelController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.TablesCount = _context.GameTables.Count();
            ViewBag.ActiveTablesCount = _context.GameTables.Count(t => t.IsActive);
            ViewBag.HallsCount = _context.Halls.Count();
            ViewBag.GameSessionsCount = _context.GameSessions.Count();

            return View();
        }

        public IActionResult GameTables()
        {
            var tables = _context.GameTables
                .Include(t => t.Hall)
                .Include(t => t.Seats)
                .ToList();

            return View(tables);
        }
    }
}
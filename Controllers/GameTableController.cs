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
    }
}
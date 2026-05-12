using GamblingBuddies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Controllers
{
    [Authorize(Roles = "Administrator,Manager")]
    public class ReservationsController : Controller
    {
        private readonly AppDbContext _context;

        public ReservationsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var reservations = _context.Reservations
                .Include(r => r.Player)
                .Include(r => r.GameSession)
                    .ThenInclude(gs => gs.GameVariant)
                .Include(r => r.ReservationStatus)
                .ToList();

            return View(reservations);
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamblingBuddies.Controllers
{
    [Authorize]
    public class HallsController : Controller
    {

        private readonly AppDbContext _context;
       
        public HallsController(AppDbContext context)
        {
            _context = context;
        }

        
        public IActionResult Halls()
        {
            return View("Halls");
        }
    }
}

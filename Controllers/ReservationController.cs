using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamblingBuddies.Controllers
{
    public class ReservationController : Controller
    {
        //[Authorize]
        public IActionResult Go()
        {
            return View("Reservation");
        }
    }
}

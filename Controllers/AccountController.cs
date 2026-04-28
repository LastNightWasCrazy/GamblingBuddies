using Microsoft.AspNetCore.Mvc;

namespace GamblingBuddies.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

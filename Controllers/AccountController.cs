using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string login, string password)
        {
            var user = _context.SystemUsers
    .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.RoleDictionary)
    .FirstOrDefault(u => u.Login == login && u.IsActive && u.IsApproved);

            if (user == null)
            {
                ViewBag.Error = "Nieprawidłowy login lub hasło";
                return View();
            }

            if (user.PasswordHash != password)
            {
                ViewBag.Error = "Nieprawidłowy login lub hasło";
                return View();
            }

            var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, user.SystemUserId.ToString()),
    new Claim(ClaimTypes.Name, user.Login)
};

            foreach (var userRole in user.UserRoles)
            {
                if (userRole.RoleDictionary != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.RoleDictionary.Name));
                }
            }

            var identity = new ClaimsIdentity(claims, "MyCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("MyCookieAuth", principal);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string login, string email, string password)
        {
            if (_context.SystemUsers.Any(u => u.Login == login || u.Email == email))
            {
                ViewBag.Error = "Login lub email już istnieje.";
                return View();
            }

            var user = new SystemUser
            {
                Login = login,
                Email = email,
                PasswordHash = password,
                IsActive = true,
                IsApproved = false,
                CreatedAt = DateTime.Now
            };

            _context.SystemUsers.Add(user);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Konto utworzone. Czeka na akceptację administratora.";
            return View();
        }
    }
}

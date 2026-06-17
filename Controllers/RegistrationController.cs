using GamblingBuddies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GamblingBuddies.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly AppDbContext _context;

        public RegistrationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegistrationRequest model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _context.SystemUsers
                .FirstOrDefaultAsync(u => u.Login == model.Login);

            if (existingUser != null)
            {
                ModelState.AddModelError("Login", "Użytkownik o tym loginie już istnieje.");
                return View(model);
            }

            var existingRequest = await _context.RegistrationRequests
                .FirstOrDefaultAsync(r => r.Login == model.Login && r.Status == "Pending");

            if (existingRequest != null)
            {
                ModelState.AddModelError("Login", "Zgłoszenie dla tego loginu już oczekuje na zatwierdzenie.");
                return View(model);
            }

            var request = new RegistrationRequest
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Phone = model.Phone,
                Login = model.Login,
                PasswordHash = model.PasswordHash,
                CreatedAt = DateTime.Now,
                Status = "Pending"
            };

            _context.RegistrationRequests.Add(request);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Twoje zgłoszenie zostało wysłane. Czekaj na zatwierdzenie przez administratora.";
            return RedirectToAction("Register");
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PendingRequests()
        {
            var requests = await _context.RegistrationRequests
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string role)
        {
            var request = await _context.RegistrationRequests
                .FirstOrDefaultAsync(r => r.RegistrationRequestId == id);

            if (request == null || request.Status != "Pending")
            {
                TempData["Error"] = "Zgłoszenie nie istnieje lub zostało już przetworzone.";
                return RedirectToAction(nameof(PendingRequests));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            int adminUserId = int.Parse(userIdClaim);

            var roleDict = await _context.RoleDictionaries
                .FirstOrDefaultAsync(r => r.Name == role);

            if (roleDict == null)
            {
                TempData["Error"] = "Wybrana rola nie istnieje.";
                return RedirectToAction(nameof(PendingRequests));
            }

            var user = new SystemUser
            {
                Login = request.Login,
                Email = $"{request.Login}@gamblingbuddies.local",
                PasswordHash = request.PasswordHash,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.SystemUsers.Add(user);
            await _context.SaveChangesAsync();

            var userRole = new UserRole
            {
                SystemUserId = user.SystemUserId,
                RoleDictionaryId = roleDict.RoleDictionaryId
            };

            _context.UserRoles.Add(userRole);

            request.Status = "Approved";
            request.ProcessedAt = DateTime.Now;
            request.ProcessedByUserId = adminUserId;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Użytkownik {request.FirstName} {request.LastName} został zatwierdzony jako {role}.";
            return RedirectToAction(nameof(PendingRequests));
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.RegistrationRequests
                .FirstOrDefaultAsync(r => r.RegistrationRequestId == id);

            if (request == null || request.Status != "Pending")
            {
                TempData["Error"] = "Zgłoszenie nie istnieje lub zostało już przetworzone.";
                return RedirectToAction(nameof(PendingRequests));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            int adminUserId = int.Parse(userIdClaim);

            request.Status = "Rejected";
            request.ProcessedAt = DateTime.Now;
            request.ProcessedByUserId = adminUserId;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Zgłoszenie {request.FirstName} {request.LastName} zostało odrzucone.";
            return RedirectToAction(nameof(PendingRequests));
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var requests = await _context.RegistrationRequests
                .Where(r => r.Status != "Pending")
                .OrderByDescending(r => r.ProcessedAt)
                .ToListAsync();

            return View(requests);
        }
    }
}
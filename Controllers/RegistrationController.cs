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
            // Logowanie do konsoli
            System.Diagnostics.Debug.WriteLine("=== REJESTRACJA ===");
            System.Diagnostics.Debug.WriteLine($"Imię: {model.FirstName}");
            System.Diagnostics.Debug.WriteLine($"Nazwisko: {model.LastName}");
            System.Diagnostics.Debug.WriteLine($"Telefon: {model.Phone}");
            System.Diagnostics.Debug.WriteLine($"Login: {model.Login}");
            System.Diagnostics.Debug.WriteLine($"Hasło: {model.PasswordHash}");
            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("=== BŁĘDY WALIDACJI ===");
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Pole: {key}, Błąd: {error.ErrorMessage}");
                    }
                }
                return View(model);
            }

            System.Diagnostics.Debug.WriteLine("=== WALIDACJA OK ===");

            var existingUser = await _context.SystemUsers
                .FirstOrDefaultAsync(u => u.Login == model.Login);

            if (existingUser != null)
            {
                System.Diagnostics.Debug.WriteLine($"Użytkownik {model.Login} już istnieje");
                ModelState.AddModelError("Login", "Użytkownik o tym loginie już istnieje.");
                return View(model);
            }

            var existingRequest = await _context.RegistrationRequests
                .FirstOrDefaultAsync(r => r.Login == model.Login && r.Status == "Pending");

            if (existingRequest != null)
            {
                System.Diagnostics.Debug.WriteLine($"Zgłoszenie dla {model.Login} już oczekuje");
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

            System.Diagnostics.Debug.WriteLine($"=== ZGŁOSZENIE ZAPISANE ID: {request.RegistrationRequestId} ===");

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
            System.Diagnostics.Debug.WriteLine($"=== ZATWIERDZANIE ===");
            System.Diagnostics.Debug.WriteLine($"ID: {id}, Rola: {role}");

            var request = await _context.RegistrationRequests
                .FirstOrDefaultAsync(r => r.RegistrationRequestId == id);

            if (request == null)
            {
                TempData["Error"] = "Zgłoszenie nie istnieje.";
                return RedirectToAction(nameof(PendingRequests));
            }

            if (request.Status != "Pending")
            {
                TempData["Error"] = "Zgłoszenie zostało już przetworzone.";
                return RedirectToAction(nameof(PendingRequests));
            }

            if (string.IsNullOrEmpty(role))
            {
                TempData["Error"] = "Wybierz rolę dla użytkownika.";
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

            var existingUser = await _context.SystemUsers
                .FirstOrDefaultAsync(u => u.Login == request.Login);

            if (existingUser != null)
            {
                TempData["Error"] = $"Użytkownik o loginie {request.Login} już istnieje.";
                return RedirectToAction(nameof(PendingRequests));
            }

            try
            {
                var newUser = new SystemUser
                {
                    Login = request.Login,
                    Email = $"{request.Login}@gamblingbuddies.local",
                    PasswordHash = request.PasswordHash,
                    IsActive = true,
                    IsApproved = true,
                    CreatedAt = DateTime.Now
                };

                _context.SystemUsers.Add(newUser);
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"Utworzono użytkownika ID: {newUser.SystemUserId}");

                var savedUser = await _context.SystemUsers.FindAsync(newUser.SystemUserId);
                if (savedUser != null && !savedUser.IsActive)
                {
                    savedUser.IsActive = true;
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"Zaktualizowano IsActive na true dla użytkownika {savedUser.Login}");
                }

                var userRole = new UserRole
                {
                    SystemUserId = newUser.SystemUserId,
                    RoleDictionaryId = roleDict.RoleDictionaryId
                };

                _context.UserRoles.Add(userRole);

                request.Status = "Approved";
                request.ProcessedAt = DateTime.Now;
                request.ProcessedByUserId = adminUserId;

                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"Zgłoszenie zatwierdzone!");

                TempData["Success"] = $"Użytkownik {request.FirstName} {request.LastName} został zatwierdzony jako {role}.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BŁĄD: {ex.Message}");
                TempData["Error"] = $"Błąd podczas zatwierdzania: {ex.Message}";
            }

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
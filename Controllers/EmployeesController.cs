using GamblingBuddies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GamblingBuddies.Controllers
{
    [Authorize(Roles = "Administrator,Manager")]
    public class EmployeesController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var employees = _context.Set<Employee>()
                .Include(e => e.SystemUser)
                .Include(e => e.Position)
                .Include(e => e.Status)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToList();

            return View(employees);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadSelectLists();

            var model = new EmployeeCreateViewModel
            {
                HireDate = DateTime.Today,
                IsActive = true
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            ValidateEmployeeCreateModel(model);

            if (!ModelState.IsValid)
            {
                LoadSelectLists(model.RoleDictionaryId, model.PositionId, model.EmployeeStatusId);
                return View(model);
            }

            var systemUser = new SystemUser
            {
                Login = model.Login.Trim(),
                Email = model.Email.Trim().ToLower(),
                PasswordHash = model.Password.Trim(),
                IsActive = model.IsActive,
                CreatedAt = DateTime.Now
            };

            _context.Set<SystemUser>().Add(systemUser);
            _context.SaveChanges();

            var userRole = new UserRole
            {
                SystemUserId = systemUser.SystemUserId,
                RoleDictionaryId = model.RoleDictionaryId
            };

            _context.Set<UserRole>().Add(userRole);

            var employee = new Employee
            {
                SystemUserId = systemUser.SystemUserId,
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
                HireDate = model.HireDate,
                PositionId = model.PositionId,
                EmployeeStatusId = model.EmployeeStatusId
            };

            _context.Set<Employee>().Add(employee);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Pracownik został dodany razem z kontem logowania.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var employee = _context.Set<Employee>()
                .Include(e => e.SystemUser)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.RoleDictionary)
                .Include(e => e.Position)
                .Include(e => e.Status)
                .FirstOrDefault(e => e.EmployeeId == id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var employee = _context.Set<Employee>()
                .Include(e => e.SystemUser)
                    .ThenInclude(u => u.UserRoles)
                .FirstOrDefault(e => e.EmployeeId == id);

            if (employee == null)
                return NotFound();

            var roleId = employee.SystemUser?.UserRoles
                .Select(ur => ur.RoleDictionaryId)
                .FirstOrDefault() ?? 0;

            var model = new EmployeeEditViewModel
            {
                EmployeeId = employee.EmployeeId,
                SystemUserId = employee.SystemUserId ?? 0,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Phone = employee.Phone,
                HireDate = employee.HireDate,
                PositionId = employee.PositionId,
                EmployeeStatusId = employee.EmployeeStatusId,
                Login = employee.SystemUser?.Login ?? "",
                Email = employee.SystemUser?.Email ?? "",
                IsActive = employee.SystemUser?.IsActive ?? true,
                RoleDictionaryId = roleId
            };

            LoadSelectLists(model.RoleDictionaryId, model.PositionId, model.EmployeeStatusId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            ValidateEmployeeEditModel(model);

            if (!ModelState.IsValid)
            {
                LoadSelectLists(model.RoleDictionaryId, model.PositionId, model.EmployeeStatusId);
                return View(model);
            }

            var employee = _context.Set<Employee>()
                .Include(e => e.SystemUser)
                    .ThenInclude(u => u.UserRoles)
                .FirstOrDefault(e => e.EmployeeId == model.EmployeeId);

            if (employee == null)
                return NotFound();

            if (employee.SystemUser == null)
            {
                TempData["ErrorMessage"] = "Ten pracownik nie ma przypisanego konta użytkownika.";
                return RedirectToAction(nameof(Index));
            }

            employee.FirstName = model.FirstName.Trim();
            employee.LastName = model.LastName.Trim();
            employee.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
            employee.HireDate = model.HireDate;
            employee.PositionId = model.PositionId;
            employee.EmployeeStatusId = model.EmployeeStatusId;

            employee.SystemUser.Login = model.Login.Trim();
            employee.SystemUser.Email = model.Email.Trim().ToLower();
            employee.SystemUser.IsActive = model.IsActive;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                employee.SystemUser.PasswordHash = model.NewPassword.Trim();
            }

            var oldRoles = employee.SystemUser.UserRoles.ToList();
            _context.Set<UserRole>().RemoveRange(oldRoles);

            _context.Set<UserRole>().Add(new UserRole
            {
                SystemUserId = employee.SystemUser.SystemUserId,
                RoleDictionaryId = model.RoleDictionaryId
            });

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Pracownik został zaktualizowany.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var employee = _context.Set<Employee>()
                .Include(e => e.SystemUser)
                    .ThenInclude(u => u.UserRoles)
                .FirstOrDefault(e => e.EmployeeId == id);

            if (employee == null)
                return NotFound();

            var hasAssignments = _context.Set<EmployeeAssignment>()
                .Any(a => a.EmployeeId == id);

            var hasWorkShifts = _context.Set<WorkShift>()
                .Any(w => w.EmployeeId == id);

            if (hasAssignments || hasWorkShifts)
            {
                employee.EmployeeStatusId = GetInactiveEmployeeStatusId();

                if (employee.SystemUser != null)
                {
                    employee.SystemUser.IsActive = false;
                }

                _context.SaveChanges();

                TempData["SuccessMessage"] = "Pracownik ma powiązane dane, więc został dezaktywowany.";
                return RedirectToAction(nameof(Index));
            }

            if (employee.SystemUser != null)
            {
                _context.Set<UserRole>().RemoveRange(employee.SystemUser.UserRoles);
                _context.Set<SystemUser>().Remove(employee.SystemUser);
            }

            _context.Set<Employee>().Remove(employee);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Pracownik został usunięty.";

            return RedirectToAction(nameof(Index));
        }

        private void LoadSelectLists(
            int? selectedRoleId = null,
            int? selectedPositionId = null,
            int? selectedStatusId = null)
        {
            ViewBag.Roles = new SelectList(
                _context.Set<RoleDictionary>()
                    .Where(r => r.IsActive && (r.Name == "Manager" || r.Name == "Employee"))
                    .OrderBy(r => r.Name)
                    .ToList(),
                "RoleDictionaryId",
                "Name",
                selectedRoleId
            );

            ViewBag.Positions = new SelectList(
                _context.Set<EmployeePositionDictionary>()
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToList(),
                "EmployeePositionDictionaryId",
                "Name",
                selectedPositionId
            );

            ViewBag.Statuses = new SelectList(
                _context.Set<EmployeeStatusDictionary>()
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .ToList(),
                "EmployeeStatusDictionaryId",
                "Name",
                selectedStatusId
            );
        }

        private void ValidateEmployeeCreateModel(EmployeeCreateViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.FirstName))
                ModelState.AddModelError("FirstName", "Imię jest wymagane.");

            if (string.IsNullOrWhiteSpace(model.LastName))
                ModelState.AddModelError("LastName", "Nazwisko jest wymagane.");

            if (string.IsNullOrWhiteSpace(model.Login))
                ModelState.AddModelError("Login", "Login jest wymagany.");

            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError("Email", "Email jest wymagany.");

            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError("Password", "Hasło jest wymagane.");

            if (model.RoleDictionaryId <= 0)
                ModelState.AddModelError("RoleDictionaryId", "Wybierz rolę użytkownika.");

            if (model.PositionId <= 0)
                ModelState.AddModelError("PositionId", "Wybierz stanowisko.");

            if (model.EmployeeStatusId <= 0)
                ModelState.AddModelError("EmployeeStatusId", "Wybierz status pracownika.");

            var loginExists = _context.Set<SystemUser>()
                .Any(u => u.Login == model.Login.Trim());

            if (loginExists)
                ModelState.AddModelError("Login", "Użytkownik z takim loginem już istnieje.");

            var emailExists = _context.Set<SystemUser>()
                .Any(u => u.Email == model.Email.Trim().ToLower());

            if (emailExists)
                ModelState.AddModelError("Email", "Użytkownik z takim emailem już istnieje.");
        }

        private void ValidateEmployeeEditModel(EmployeeEditViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.FirstName))
                ModelState.AddModelError("FirstName", "Imię jest wymagane.");

            if (string.IsNullOrWhiteSpace(model.LastName))
                ModelState.AddModelError("LastName", "Nazwisko jest wymagane.");

            if (string.IsNullOrWhiteSpace(model.Login))
                ModelState.AddModelError("Login", "Login jest wymagany.");

            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError("Email", "Email jest wymagany.");

            if (model.RoleDictionaryId <= 0)
                ModelState.AddModelError("RoleDictionaryId", "Wybierz rolę użytkownika.");

            if (model.PositionId <= 0)
                ModelState.AddModelError("PositionId", "Wybierz stanowisko.");

            if (model.EmployeeStatusId <= 0)
                ModelState.AddModelError("EmployeeStatusId", "Wybierz status pracownika.");

            var loginExists = _context.Set<SystemUser>()
                .Any(u =>
                    u.SystemUserId != model.SystemUserId &&
                    u.Login == model.Login.Trim());

            if (loginExists)
                ModelState.AddModelError("Login", "Użytkownik z takim loginem już istnieje.");

            var emailExists = _context.Set<SystemUser>()
                .Any(u =>
                    u.SystemUserId != model.SystemUserId &&
                    u.Email == model.Email.Trim().ToLower());

            if (emailExists)
                ModelState.AddModelError("Email", "Użytkownik z takim emailem już istnieje.");
        }

        private int GetInactiveEmployeeStatusId()
        {
            var inactiveStatus = _context.Set<EmployeeStatusDictionary>()
                .FirstOrDefault(s => s.Name == "Inactive");

            if (inactiveStatus != null)
                return inactiveStatus.EmployeeStatusDictionaryId;

            var anyStatus = _context.Set<EmployeeStatusDictionary>()
                .OrderBy(s => s.EmployeeStatusDictionaryId)
                .First();

            return anyStatus.EmployeeStatusDictionaryId;
        }
    }

    public class EmployeeCreateViewModel
    {
        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        public string? Phone { get; set; }

        [Required]
        public DateTime HireDate { get; set; }

        [Required]
        public int PositionId { get; set; }

        [Required]
        public int EmployeeStatusId { get; set; }

        [Required]
        public string Login { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        [Required]
        public int RoleDictionaryId { get; set; }

        public bool IsActive { get; set; }
    }

    public class EmployeeEditViewModel
    {
        public int EmployeeId { get; set; }

        public int SystemUserId { get; set; }

        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        public string? Phone { get; set; }

        [Required]
        public DateTime HireDate { get; set; }

        [Required]
        public int PositionId { get; set; }

        [Required]
        public int EmployeeStatusId { get; set; }

        [Required]
        public string Login { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public string? NewPassword { get; set; }

        [Required]
        public int RoleDictionaryId { get; set; }

        public bool IsActive { get; set; }
    }
}
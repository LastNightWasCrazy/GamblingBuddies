using GamblingBuddies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GamblingBuddies.Controllers
{
    [Authorize(Roles = "Administrator,Manager,Employee")]
    public class EmployeesScheduleController : Controller
    {
        private readonly AppDbContext _context;

        public EmployeesScheduleController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult WorkShifts(bool onlyMine = false, bool showInactive = false)
        {
            var isAdminOrManager = User.IsInRole("Administrator") || User.IsInRole("Manager");

            if (!isAdminOrManager)
            {
                onlyMine = true;
            }

            var inactiveStatusId = GetEmployeeStatusId("Inactive");

            var query = _context.WorkShifts
                .Include(ws => ws.Employee)
                    .ThenInclude(e => e.Position)
                .Include(ws => ws.Employee)
                    .ThenInclude(e => e.Status)
                .Include(ws => ws.CreatedByUser)
                .AsQueryable();

            if (!showInactive && inactiveStatusId.HasValue)
            {
                query = query.Where(ws => ws.Employee != null && ws.Employee.EmployeeStatusId != inactiveStatusId.Value);
            }

            if (onlyMine)
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                    return Unauthorized();

                query = query.Where(ws => ws.Employee != null && ws.Employee.SystemUserId == currentUserId.Value);
            }

            var workShifts = query
                .OrderBy(ws => ws.StartAt)
                .ToList();

            ViewBag.OnlyMine = onlyMine;
            ViewBag.ShowInactive = showInactive;
            ViewBag.IsAdminOrManager = isAdminOrManager;

            return View(workShifts);
        }

        public IActionResult Assignments(bool onlyMine = false, bool showInactive = false)
        {
            var isAdminOrManager = User.IsInRole("Administrator") || User.IsInRole("Manager");

            if (!isAdminOrManager)
            {
                onlyMine = true;
            }

            var inactiveStatusId = GetEmployeeStatusId("Inactive");

            var query = _context.EmployeeAssignments
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Position)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Status)
                .Include(a => a.GameSession)
                    .ThenInclude(gs => gs.GameVariant)
                        .ThenInclude(gv => gv.Game)
                .Include(a => a.GameSession)
                    .ThenInclude(gs => gs.GameTable)
                        .ThenInclude(gt => gt.Hall)
                .Include(a => a.AssignedByUser)
                .AsQueryable();

            if (!showInactive && inactiveStatusId.HasValue)
            {
                query = query.Where(a => a.Employee != null && a.Employee.EmployeeStatusId != inactiveStatusId.Value);
            }

            if (onlyMine)
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                    return Unauthorized();

                query = query.Where(a => a.Employee != null && a.Employee.SystemUserId == currentUserId.Value);
            }

            var assignments = query
                .OrderBy(a => a.GameSession.StartAt)
                .ToList();

            ViewBag.OnlyMine = onlyMine;
            ViewBag.ShowInactive = showInactive;
            ViewBag.IsAdminOrManager = isAdminOrManager;

            return View(assignments);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet]
        public IActionResult CreateWorkShift()
        {
            LoadWorkShiftSelectLists();
            return View(new WorkShiftFormViewModel
            {
                StartAt = DateTime.Now.AddHours(1),
                EndAt = DateTime.Now.AddHours(9)
            });
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateWorkShift(WorkShiftFormViewModel model)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return Unauthorized();

            NormalizeWorkShiftTimes(model);
            ValidateWorkShiftForm(model);

            if (!ModelState.IsValid)
            {
                LoadWorkShiftSelectLists(model.EmployeeId);
                return View(model);
            }

            try
            {
                var workShift = new WorkShift
                {
                    EmployeeId = model.EmployeeId,
                    StartAt = model.StartAt,
                    EndAt = model.EndAt,
                    CreatedByUserId = currentUserId.Value
                };

                _context.WorkShifts.Add(workShift);
                _context.SaveChanges();

                TempData["Success"] = "Zmiana została dodana.";
                return RedirectToAction(nameof(WorkShifts));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Nie udało się zapisać zmiany: " + GetFullExceptionMessage(ex));
                LoadWorkShiftSelectLists(model.EmployeeId);
                return View(model);
            }
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet]
        public IActionResult CreateAssignment()
        {
            LoadAssignmentSelectLists();
            return View(new AssignmentFormViewModel());
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAssignment(AssignmentFormViewModel model)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return Unauthorized();

            ValidateAssignmentForm(model);

            if (!ModelState.IsValid)
            {
                LoadAssignmentSelectLists(model.EmployeeId, model.GameSessionId);
                return View(model);
            }

            try
            {
                var assignment = new EmployeeAssignment
                {
                    EmployeeId = model.EmployeeId,
                    GameSessionId = model.GameSessionId,
                    Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
                    AssignedByUserId = currentUserId.Value
                };

                _context.EmployeeAssignments.Add(assignment);
                _context.SaveChanges();

                TempData["Success"] = "Przypisanie zostało dodane.";
                return RedirectToAction(nameof(Assignments));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Nie udało się zapisać przypisania: " + GetFullExceptionMessage(ex));
                LoadAssignmentSelectLists(model.EmployeeId, model.GameSessionId);
                return View(model);
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWorkShift(int id)
        {
            var workShift = await _context.WorkShifts.FindAsync(id);
            if (workShift == null)
                return NotFound();

            _context.WorkShifts.Remove(workShift);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Zmiana została usunięta.";
            return RedirectToAction(nameof(WorkShifts));
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var assignment = await _context.EmployeeAssignments.FindAsync(id);
            if (assignment == null)
                return NotFound();

            _context.EmployeeAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Przypisanie zostało usunięte.";
            return RedirectToAction(nameof(Assignments));
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int systemUserId))
                return null;

            var exists = _context.SystemUsers.Any(u => u.SystemUserId == systemUserId);
            return exists ? systemUserId : null;
        }

        private int? GetEmployeeStatusId(string statusName)
        {
            return _context.EmployeeStatusDictionaries
                .Where(s => s.Name == statusName)
                .Select(s => (int?)s.EmployeeStatusDictionaryId)
                .FirstOrDefault();
        }

        private void ValidateWorkShiftForm(WorkShiftFormViewModel model)
        {
            var employee = _context.Employees
                .Include(e => e.Status)
                .FirstOrDefault(e => e.EmployeeId == model.EmployeeId);

            if (employee == null)
            {
                ModelState.AddModelError(nameof(model.EmployeeId), "Wybierz poprawnego pracownika.");
            }
            else if (employee.Status != null && employee.Status.Name == "Inactive")
            {
                ModelState.AddModelError(nameof(model.EmployeeId), "Nie można dodać zmiany dla nieaktywnego pracownika.");
            }

            if (model.StartAt == default)
                ModelState.AddModelError(nameof(model.StartAt), "Podaj początek zmiany.");

            if (model.EndAt == default)
                ModelState.AddModelError(nameof(model.EndAt), "Podaj koniec zmiany.");

            if (model.EndAt <= model.StartAt)
            {
                ModelState.AddModelError(nameof(model.EndAt), "Koniec zmiany musi być później niż początek zmiany.");
                return;
            }

            if (model.EndAt - model.StartAt > TimeSpan.FromHours(12))
            {
                ModelState.AddModelError(nameof(model.EndAt), "Zmiana nie może trwać dłużej niż 12 godzin.");
            }

            var overlapExists = _context.WorkShifts.Any(ws =>
                ws.EmployeeId == model.EmployeeId &&
                ws.StartAt < model.EndAt &&
                model.StartAt < ws.EndAt);

            if (overlapExists)
            {
                ModelState.AddModelError("", "Ten pracownik ma już zmianę w tym czasie. Zmiany nie mogą się nakładać.");
                return;
            }

            var tooShortBreakExists = _context.WorkShifts.Any(ws =>
                ws.EmployeeId == model.EmployeeId &&
                ws.StartAt < model.EndAt.AddHours(8) &&
                model.StartAt < ws.EndAt.AddHours(8));

            if (tooShortBreakExists)
            {
                ModelState.AddModelError("", "Pracownik musi mieć minimum 8 godzin przerwy między zmianami. Zmiany mogą być jedna po drugiej dopiero po 8 godzinach odstępu.");
            }
        }

        private void ValidateAssignmentForm(AssignmentFormViewModel model)
        {
            var employee = _context.Employees
                .Include(e => e.Status)
                .FirstOrDefault(e => e.EmployeeId == model.EmployeeId);

            if (employee == null)
            {
                ModelState.AddModelError(nameof(model.EmployeeId), "Wybierz poprawnego pracownika.");
            }
            else if (employee.Status != null && employee.Status.Name == "Inactive")
            {
                ModelState.AddModelError(nameof(model.EmployeeId), "Nie można przypisać nieaktywnego pracownika do sesji gry.");
            }

            var selectedSession = _context.GameSessions
                .FirstOrDefault(gs => gs.GameSessionId == model.GameSessionId);

            if (selectedSession == null)
            {
                ModelState.AddModelError(nameof(model.GameSessionId), "Wybierz poprawną sesję gry.");
                return;
            }

            if (selectedSession.EndAt <= selectedSession.StartAt)
            {
                ModelState.AddModelError(nameof(model.GameSessionId), "Wybrana sesja gry ma niepoprawny czas trwania.");
                return;
            }

            // Pracownik może zostać przypisany do sesji tylko wtedy,
            // gdy cała sesja mieści się w jego zmianie pracy.
            // Przykład OK: zmiana 08:00-16:00, sesja 10:00-12:00.
            // Przykład NIE: zmiana 08:00-16:00, sesja 15:00-18:00.
            var matchingWorkShiftExists = _context.WorkShifts.Any(ws =>
                ws.EmployeeId == model.EmployeeId &&
                ws.StartAt <= selectedSession.StartAt &&
                selectedSession.EndAt <= ws.EndAt);

            if (!matchingWorkShiftExists)
            {
                ModelState.AddModelError("", "Nie można przypisać pracownika do tej sesji, ponieważ czas sesji nie mieści się w żadnej jego zmianie pracy.");
            }

            var duplicateExists = _context.EmployeeAssignments
                .Any(a => a.EmployeeId == model.EmployeeId && a.GameSessionId == model.GameSessionId);

            if (duplicateExists)
            {
                ModelState.AddModelError("", "Ten pracownik jest już przypisany do tej sesji.");
            }

            var overlappingAssignmentExists = _context.EmployeeAssignments
                .Include(a => a.GameSession)
                .Any(a =>
                    a.EmployeeId == model.EmployeeId &&
                    a.GameSessionId != model.GameSessionId &&
                    a.GameSession.StartAt < selectedSession.EndAt &&
                    selectedSession.StartAt < a.GameSession.EndAt);

            if (overlappingAssignmentExists)
            {
                ModelState.AddModelError("", "Ten pracownik ma już przypisanie do innej sesji w tym czasie. Przypisania mogą być jedno po drugim, ale nie mogą się nakładać.");
            }
        }

        private void NormalizeWorkShiftTimes(WorkShiftFormViewModel model)
        {
            model.StartAt = new DateTime(
                model.StartAt.Year,
                model.StartAt.Month,
                model.StartAt.Day,
                model.StartAt.Hour,
                model.StartAt.Minute,
                0);

            model.EndAt = new DateTime(
                model.EndAt.Year,
                model.EndAt.Month,
                model.EndAt.Day,
                model.EndAt.Hour,
                model.EndAt.Minute,
                0);
        }

        private void LoadWorkShiftSelectLists(int? selectedEmployeeId = null)
        {
            ViewBag.Employees = new SelectList(GetActiveEmployees(), "EmployeeId", "FullName", selectedEmployeeId);
        }

        private void LoadAssignmentSelectLists(int? selectedEmployeeId = null, int? selectedGameSessionId = null)
        {
            ViewBag.Employees = new SelectList(GetActiveEmployees(), "EmployeeId", "FullName", selectedEmployeeId);
            ViewBag.GameSessions = new SelectList(GetGameSessionOptions(), "GameSessionId", "Label", selectedGameSessionId);
        }

        private List<EmployeeOption> GetActiveEmployees()
        {
            var inactiveStatusId = GetEmployeeStatusId("Inactive");

            var query = _context.Employees
                .Include(e => e.Status)
                .AsQueryable();

            if (inactiveStatusId.HasValue)
            {
                query = query.Where(e => e.EmployeeStatusId != inactiveStatusId.Value);
            }

            return query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Select(e => new EmployeeOption
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.FirstName + " " + e.LastName
                })
                .ToList();
        }

        private List<GameSessionOption> GetGameSessionOptions()
        {
            return _context.GameSessions
                .Include(gs => gs.GameVariant)
                    .ThenInclude(gv => gv.Game)
                .Include(gs => gs.GameTable)
                    .ThenInclude(gt => gt.Hall)
                .OrderBy(gs => gs.StartAt)
                .AsEnumerable()
                .Select(gs => new GameSessionOption
                {
                    GameSessionId = gs.GameSessionId,
                    Label = $"{gs.GameVariant?.Game?.Name ?? "Gra"} - {gs.GameVariant?.Name ?? "Wariant"} | {gs.GameTable?.Hall?.Name ?? "Sala"}, stół {gs.GameTable?.TableNumber.ToString() ?? "-"} | {gs.StartAt:dd.MM.yyyy HH:mm}"
                })
                .ToList();
        }

        private string GetFullExceptionMessage(Exception ex)
        {
            var messages = new List<string>();
            var current = ex;

            while (current != null)
            {
                messages.Add(current.Message);
                current = current.InnerException;
            }

            return string.Join(" | ", messages);
        }

        private class EmployeeOption
        {
            public int EmployeeId { get; set; }
            public string FullName { get; set; } = "";
        }

        private class GameSessionOption
        {
            public int GameSessionId { get; set; }
            public string Label { get; set; } = "";
        }
    }

    public class WorkShiftFormViewModel
    {
        [Required(ErrorMessage = "Wybierz pracownika.")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Podaj początek zmiany.")]
        public DateTime StartAt { get; set; }

        [Required(ErrorMessage = "Podaj koniec zmiany.")]
        public DateTime EndAt { get; set; }
    }

    public class AssignmentFormViewModel
    {
        [Required(ErrorMessage = "Wybierz pracownika.")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Wybierz sesję gry.")]
        public int GameSessionId { get; set; }

        [StringLength(1000, ErrorMessage = "Notatka może mieć maksymalnie 1000 znaków.")]
        public string? Notes { get; set; }
    }
}

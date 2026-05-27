using GamblingBuddies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        public IActionResult WorkShifts(bool onlyMine = false)
        {
            var query = _context.Set<WorkShift>()
        .Include(ws => ws.Employee)
            .ThenInclude(e => e.Position)
        .Include(ws => ws.Employee)
            .ThenInclude(e => e.Status)
        .Include(ws => ws.CreatedByUser)
        .AsQueryable();

            if (onlyMine)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (int.TryParse(userIdClaim, out int systemUserId))
                {
                    query = query.Where(ws => ws.Employee.SystemUserId == systemUserId);
                }
            }

            var workShifts = query
                .OrderBy(ws => ws.StartAt)
                .ToList();

            ViewBag.OnlyMine = onlyMine;

            return View(workShifts);
        }

        public IActionResult Assignments()
        {
            var assignments = _context.Set<EmployeeAssignment>()
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Position)
                .Include(a => a.GameSession)
                    .ThenInclude(gs => gs.GameVariant)
                        .ThenInclude(gv => gv.Game)
                .Include(a => a.GameSession)
                    .ThenInclude(gs => gs.GameTable)
                        .ThenInclude(gt => gt.Hall)
                .Include(a => a.AssignedByUser)
                .OrderBy(a => a.GameSession.StartAt)
                .ToList();

            return View(assignments);
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet]
        public IActionResult CreateWorkShift() 
        {
            ViewBag.Employees = _context.Set<Employee>()
                .OrderBy(e => e.LastName)
                .ToList();
            return View();
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateWorkShift(WorkShift workShift)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int systemUserId))
            {
                return Unauthorized();
            }

            ModelState.Remove("Employee");
            ModelState.Remove("CreatedByUser");

            workShift.CreatedByUserId = systemUserId;

            if (!_context.Set<Employee>().Any(e => e.EmployeeId == workShift.EmployeeId))
            {
                ModelState.AddModelError("EmployeeId", "Wybierz poprawnego pracownika.");
            }

            if (workShift.EndAt <= workShift.StartAt)
            {
                ModelState.AddModelError("", "Koniec zmiany musi być później niż początek zmiany.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Employees = _context.Set<Employee>()
                    .OrderBy(e => e.LastName)
                    .ToList();

                return View(workShift);
            }

            _context.Set<WorkShift>().Add(workShift);
            _context.SaveChanges();

            return RedirectToAction(nameof(WorkShifts));
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet]
        public IActionResult CreateAssignment()
        {
            ViewBag.Employees = _context.Set<Employee>()
                .OrderBy(e => e.LastName)
                .ToList();

            ViewBag.GameSessions = _context.Set<GameSession>()
                .Include(gs => gs.GameVariant)
                    .ThenInclude(gv => gv.Game)
                .Include(gs => gs.GameTable)
                    .ThenInclude(gt => gt.Hall)
                .OrderBy(gs => gs.StartAt)
                .ToList();

            return View();
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAssignment(EmployeeAssignment assignment)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int systemUserId))
            {
                return Unauthorized();
            }

            assignment.AssignedByUserId = systemUserId;

            ModelState.Remove("Employee");
            ModelState.Remove("GameSession");
            ModelState.Remove("AssignedByUser");

            if (!ModelState.IsValid)
            {
                ViewBag.Employees = _context.Set<Employee>()
                    .OrderBy(e => e.LastName)
                    .ToList();

                ViewBag.GameSessions = _context.Set<GameSession>()
                    .Include(gs => gs.GameVariant)
                        .ThenInclude(gv => gv.Game)
                    .Include(gs => gs.GameTable)
                        .ThenInclude(gt => gt.Hall)
                    .OrderBy(gs => gs.StartAt)
                    .ToList();

                return View(assignment);
            }



            _context.Set<EmployeeAssignment>().Add(assignment);
            _context.SaveChanges();

            return RedirectToAction(nameof(Assignments));
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWorkShift(int id)
        {
            var workShift = await _context.WorkShifts.FindAsync(id);
            if (workShift == null)
            {
                return NotFound();
            }

            _context.WorkShifts.Remove(workShift);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Zmiana została usunięta.";

            return RedirectToAction(nameof(WorkShifts));
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var assignment = await _context.EmployeeAssignments.FindAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            _context.EmployeeAssignments.Remove(assignment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Przypisanie zostało usunięte.";

            return RedirectToAction(nameof(Assignments));
        }
    }
}
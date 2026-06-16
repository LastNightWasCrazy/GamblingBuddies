using GamblingBuddies.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamblingBuddies.Controllers
{
    [Authorize(Roles = "Administrator,Manager")]
    public class PaymentsController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 20;

            if (page < 1)
            {
                page = 1;
            }

            var totalPayments = await _context.Payments.CountAsync();

            var totalPages = (int)Math.Ceiling(totalPayments / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            var payments = await _context.Payments
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Player)
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentStatus)
                .Include(p => p.PaymentTransactions)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalPayments = totalPayments;
            ViewBag.PageSize = pageSize;

            return View(payments);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Player)
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.GameSession)
                .Include(p => p.PaymentMethod)
                .Include(p => p.PaymentStatus)
                .Include(p => p.PaymentTransactions)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.PaymentTransactions)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
            {
                return NotFound();
            }

            try
            {
                if (payment.PaymentTransactions != null && payment.PaymentTransactions.Any())
                {
                    _context.PaymentTransactions.RemoveRange(payment.PaymentTransactions);
                }

                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Płatność została usunięta.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Błąd podczas usuwania: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
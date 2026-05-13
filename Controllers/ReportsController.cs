using GamblingBuddies.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GamblingBuddies.Controllers
{
    [Authorize(Roles = "Administrator,Manager")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PaymentReport()
        {
            ViewBag.PaymentMethods = await _context.PaymentMethods.ToListAsync();
            ViewBag.PaymentStatuses = await _context.PaymentStatuses.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePaymentReport(DateTime? startDate, DateTime? endDate, string? paymentMethod, string? status)
        {
            try
            {
                var query = _context.Payments
                    .Include(p => p.PaymentMethod)
                    .Include(p => p.PaymentStatus)
                    .Include(p => p.Reservation)
                        .ThenInclude(r => r.Player)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(p => p.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(p => p.CreatedAt <= endDate.Value);

                if (!string.IsNullOrEmpty(paymentMethod))
                    query = query.Where(p => p.PaymentMethod != null && p.PaymentMethod.Name == paymentMethod);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(p => p.PaymentStatus != null && p.PaymentStatus.Name == status);

                var payments = await query.ToListAsync();

                decimal totalAmount = payments.Sum(p => p.Amount);
                int transactionsCount = payments.Count;
                decimal averageAmount = transactionsCount > 0 ? totalAmount / transactionsCount : 0;

                byte[] pdfBytes = GeneratePdfReport(payments, startDate, endDate, paymentMethod, status, totalAmount, transactionsCount, averageAmount);

                int userId = GetCurrentUserId();

                var paymentReport = new PaymentReport
                {
                    ReportName = $"Raport płatności_{DateTime.Now:yyyyMMdd_HHmmss}",
                    Description = $"Raport płatności za okres {startDate?.ToShortDateString() ?? "od początku"} - {endDate?.ToShortDateString() ?? "do teraz"}",
                    GeneratedDate = DateTime.Now,
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalAmount = totalAmount,
                    TransactionsCount = transactionsCount,
                    FiltersApplied = $"{{\"paymentMethod\":\"{paymentMethod}\",\"status\":\"{status}\"}}",
                    GeneratedByUserId = userId,
                    PdfFilePath = null
                };

                _context.PaymentReports.Add(paymentReport);
                await _context.SaveChangesAsync();

                return File(pdfBytes, "application/pdf", $"Raport_platnosci_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Wystąpił błąd podczas generowania raportu: {ex.Message}";
                return RedirectToAction(nameof(PaymentReport));
            }
        }

        public async Task<IActionResult> ReportHistory()
        {
            var reports = await _context.PaymentReports
                .Include(r => r.GeneratedByUser)
                .OrderByDescending(r => r.GeneratedDate)
                .ToListAsync();

            return View(reports);
        }

        public async Task<IActionResult> Details(int id)
        {
            var report = await _context.PaymentReports
                .Include(r => r.GeneratedByUser)
                .FirstOrDefaultAsync(r => r.PaymentReportId == id);

            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _context.PaymentReports.FindAsync(id);
            if (report != null)
            {
                _context.PaymentReports.Remove(report);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Raport został usunięty.";
            }

            return RedirectToAction(nameof(ReportHistory));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return 1;
            }
            return int.Parse(userIdClaim);
        }

        private byte[] GeneratePdfReport(List<Payment> payments, DateTime? startDate, DateTime? endDate,
            string? paymentMethod, string? status, decimal totalAmount, int transactionsCount, decimal averageAmount)
        {
            using (var memoryStream = new MemoryStream())
            {
                iTextSharp.text.Document document = new iTextSharp.text.Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                iTextSharp.text.Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(139, 0, 0));
                iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph("RAPORT PŁATNOŚCI KASYNA", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                document.Add(new iTextSharp.text.Paragraph(" "));

                iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                iTextSharp.text.Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

                document.Add(new iTextSharp.text.Paragraph("Informacje o raporcie:", boldFont));
                document.Add(new iTextSharp.text.Paragraph($"Data generowania: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", normalFont));
                document.Add(new iTextSharp.text.Paragraph($"Okres od: {startDate?.ToShortDateString() ?? "od początku"} do: {endDate?.ToShortDateString() ?? "do teraz"}", normalFont));
                document.Add(new iTextSharp.text.Paragraph($"Metoda płatności: {paymentMethod ?? "Wszystkie"}", normalFont));
                document.Add(new iTextSharp.text.Paragraph($"Status: {status ?? "Wszystkie"}", normalFont));
                document.Add(new iTextSharp.text.Paragraph(" "));

                document.Add(new iTextSharp.text.Paragraph("Podsumowanie statystyczne:", boldFont));
                document.Add(new iTextSharp.text.Paragraph($"Liczba transakcji: {transactionsCount}", normalFont));
                document.Add(new iTextSharp.text.Paragraph($"Łączna wartość: {totalAmount:C}", normalFont));
                document.Add(new iTextSharp.text.Paragraph($"Średnia wartość transakcji: {averageAmount:C}", normalFont));
                document.Add(new iTextSharp.text.Paragraph(" "));

                document.Add(new iTextSharp.text.Paragraph("Szczegółowa lista płatności:", boldFont));
                document.Add(new iTextSharp.text.Paragraph(" "));

                PdfPTable table = new PdfPTable(7);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 10f, 15f, 20f, 15f, 15f, 15f, 10f });

                iTextSharp.text.Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, new BaseColor(255, 255, 255));
                iTextSharp.text.BaseColor headerBg = new BaseColor(139, 0, 0);

                AddCellToTable(table, "ID", headerFont, headerBg);
                AddCellToTable(table, "Data utworzenia", headerFont, headerBg);
                AddCellToTable(table, "Gracz", headerFont, headerBg);
                AddCellToTable(table, "Kwota", headerFont, headerBg);
                AddCellToTable(table, "Metoda", headerFont, headerBg);
                AddCellToTable(table, "Status", headerFont, headerBg);
                AddCellToTable(table, "Rezerwacja ID", headerFont, headerBg);

                iTextSharp.text.Font cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);

                foreach (var payment in payments)
                {
                    string playerName = "Brak danych";
                    if (payment.Reservation?.Player != null)
                    {
                        playerName = $"{payment.Reservation.Player.FirstName} {payment.Reservation.Player.LastName}";
                    }

                    AddCellToTable(table, payment.PaymentId.ToString(), cellFont);
                    AddCellToTable(table, payment.CreatedAt.ToString("yyyy-MM-dd HH:mm"), cellFont);
                    AddCellToTable(table, playerName, cellFont);
                    AddCellToTable(table, $"{payment.Amount:C}", cellFont);
                    AddCellToTable(table, payment.PaymentMethod?.Name ?? "Brak", cellFont);
                    AddCellToTable(table, payment.PaymentStatus?.Name ?? "Brak", cellFont);
                    AddCellToTable(table, payment.ReservationId.ToString(), cellFont);
                }

                document.Add(table);

                document.Add(new iTextSharp.text.Paragraph(" "));
                document.Add(new iTextSharp.text.Paragraph(" "));
                iTextSharp.text.Font footerFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8, new BaseColor(128, 128, 128));
                iTextSharp.text.Paragraph footer = new iTextSharp.text.Paragraph("Wygenerowano przez system zarządzania kasynem GamblingBuddies", footerFont);
                footer.Alignment = Element.ALIGN_CENTER;
                document.Add(footer);

                document.Close();
                writer.Close();

                return memoryStream.ToArray();
            }
        }

        private void AddCellToTable(PdfPTable table, string text, iTextSharp.text.Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text ?? "", font));
            cell.Border = Rectangle.BOX;
            cell.BorderColor = new BaseColor(211, 211, 211);
            cell.Padding = 5;
            table.AddCell(cell);
        }

        private void AddCellToTable(PdfPTable table, string text, iTextSharp.text.Font font, iTextSharp.text.BaseColor backgroundColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text ?? "", font));
            cell.BackgroundColor = backgroundColor;
            cell.Border = Rectangle.BOX;
            cell.BorderColor = new BaseColor(211, 211, 211);
            cell.Padding = 5;
            table.AddCell(cell);
        }
    }
}
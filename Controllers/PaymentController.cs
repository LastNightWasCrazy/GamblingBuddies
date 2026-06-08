using GamblingBuddies.Models;
using GamblingBuddies.Services.PayU;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GamblingBuddies.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PayUService _payUService;

        public PaymentController(AppDbContext context, PayUService payUService)
        {
            _context = context;
            _payUService = payUService;
        }

        [HttpGet]
        public async Task<IActionResult> Pay(int reservationId)
        {
            var payment = await _context.Payments
                .Include(p => p.Reservation)
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId);

            if (payment == null)
            {
                TempData["Error"] = "Nie znaleziono płatności.";
                return RedirectToAction("Go", "Reservation");
            }

            var continueUrl = Url.Action(
                "Continue",
                "Payment",
                new { reservationId = reservationId },
                Request.Scheme
            )!;

            var notifyUrl = Url.Action(
                "Notify",
                "Payment",
                null,
                Request.Scheme
            )!;

            var customerIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            var result = await _payUService.CreateOrderAsync(
                payment,
                continueUrl,
                notifyUrl,
                customerIp
            );

            await _context.SaveChangesAsync();

            if (string.IsNullOrEmpty(result.RedirectUri))
            {
                TempData["Error"] = "PayU nie zwróciło adresu płatności.";
                return RedirectToAction("Go", "Reservation");
            }

            return Redirect(result.RedirectUri);
        }

        [HttpGet]
        public IActionResult Continue(int reservationId)
        {
            TempData["Success"] = "Wrócono z PayU. Płatność oczekuje na potwierdzenie.";

            return RedirectToAction("Go", "Reservation");
        }

        [HttpPost]
        public async Task<IActionResult> Notify()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            using var document = JsonDocument.Parse(body);

            var order = document.RootElement.GetProperty("order");

            var orderId = order.GetProperty("orderId").GetString();
            var status = order.GetProperty("status").GetString();

            var extOrderId = order.GetProperty("extOrderId").GetString();

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p =>
                    p.PaymentProviderOrderId == orderId ||
                    p.ExternalOrderId == extOrderId);

            if (payment == null)
            {
                return Ok();
            }

            _context.PaymentTransactions.Add(new PaymentTransaction
            {
                PaymentId = payment.PaymentId,
                ExternalTransactionId = orderId ?? "BRAK_ORDER_ID",
                ProviderResponseCode = status ?? "BRAK_STATUSU",
                ProviderResponseMessage = body,
                CreatedAt = DateTime.Now
            });

            if (status == "COMPLETED")
            {
                var paidStatus = await _context.PaymentStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Paid");

                if (paidStatus != null)
                {
                    payment.PaymentStatusId = paidStatus.PaymentStatusId;
                    payment.PaidAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(int reservationId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId);

            if (payment == null)
            {
                TempData["Error"] = "Nie znaleziono płatności.";
                return RedirectToAction("Go", "Reservation");
            }

            var paidStatus = await _context.PaymentStatuses
                .FirstOrDefaultAsync(s => s.Name == "Paid");

            if (paidStatus == null)
            {
                TempData["Error"] = "Brak statusu płatności Paid w bazie.";
                return RedirectToAction("Go", "Reservation");
            }

            payment.PaymentStatusId = paidStatus.PaymentStatusId;
            payment.PaidAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Płatność kartą została potwierdzona.";
            return RedirectToAction("Go", "Reservation");
        }
    }
}
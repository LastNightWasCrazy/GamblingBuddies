using GamblingBuddies.Models;
using GamblingBuddies.Services.PayU;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GamblingBuddies.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IPayUService _payUService;
        private readonly IConfiguration _configuration;

        public PaymentController(AppDbContext context, IPayUService payUService, IConfiguration configuration)
        {
            _context = context;
            _payUService = payUService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Pay(int reservationId)
        {
            if (reservationId <= 0)
            {
                TempData["Error"] = "Niepoprawne ID rezerwacji.";
                return RedirectToAction("Go", "Reservation");
            }

            var payment = await _context.Payments
                .Include(p => p.Reservation)
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId);

            if (payment == null)
            {
                TempData["Error"] = "Nie znaleziono płatności.";
                return RedirectToAction("Go", "Reservation");
            }

            if (payment.Amount <= 0)
            {
                TempData["Error"] = "Kwota płatności musi być większa od zera.";
                return RedirectToAction("Go", "Reservation");
            }

            var publicUrl = _configuration["AppSettings:PublicUrl"]?.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(publicUrl))
            {
                TempData["Error"] = "Brak AppSettings:PublicUrl w appsettings.json.";
                return RedirectToAction("Go", "Reservation");
            }

            var continueUrl = $"{publicUrl}/Payment/Continue?reservationId={reservationId}";
            var notifyUrl = $"{publicUrl}/Payment/Notify";
            var customerIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            try
            {
                var result = await _payUService.CreateOrderAsync(
                    payment,
                    continueUrl,
                    notifyUrl,
                    customerIp
                );

                await _context.SaveChangesAsync();

                if (string.IsNullOrWhiteSpace(result.RedirectUri))
                {
                    TempData["Error"] = "PayU nie zwróciło adresu płatności.";
                    return RedirectToAction("Go", "Reservation");
                }

                return Redirect(result.RedirectUri);
            }
            catch (Exception ex)
            {
                await SetPaymentRejected(
                    payment,
                    "PAYU_CREATE_ORDER_ERROR",
                    ex.Message
                );

                TempData["Error"] = "Nie udało się rozpocząć płatności PayU.";
                return RedirectToAction("Go", "Reservation");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Continue(int reservationId)
        {
            var paymentExists = await _context.Payments
                .AnyAsync(p => p.ReservationId == reservationId);

            if (!paymentExists)
            {
                TempData["Error"] = "Nie znaleziono płatności.";
                return RedirectToAction("Go", "Reservation");
            }

            TempData["Success"] = "Wrócono z PayU. Status płatności zostanie zaktualizowany po powiadomieniu z PayU.";
            return RedirectToAction("Go", "Reservation");
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet]
        public async Task<IActionResult> Reject(int reservationId)
        {
            if (reservationId <= 0)
            {
                TempData["Error"] = "Niepoprawne ID rezerwacji.";
                return RedirectToAction("Go", "Reservation");
            }

            var payment = await _context.Payments
                .Include(p => p.Reservation)
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId);

            if (payment == null)
            {
                TempData["Error"] = "Nie znaleziono płatności.";
                return RedirectToAction("Go", "Reservation");
            }

            await SetPaymentRejected(
                payment,
                "MANUAL_REJECT",
                "Płatność została ręcznie odrzucona w systemie."
            );

            TempData["Error"] = "Płatność została odrzucona.";
            return RedirectToAction("Go", "Reservation");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Notify()
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                return BadRequest("Puste body webhooka PayU.");
            }

            if (!IsValidPayUSignature(body))
            {
                return Unauthorized("Niepoprawny podpis PayU.");
            }

            JsonDocument document;

            try
            {
                document = JsonDocument.Parse(body);
            }
            catch
            {
                return BadRequest("Niepoprawny JSON z PayU.");
            }

            using (document)
            {
                if (!document.RootElement.TryGetProperty("order", out var order))
                {
                    return BadRequest("Brak pola order w webhooku PayU.");
                }

                var orderId = order.TryGetProperty("orderId", out var orderIdElement)
                    ? orderIdElement.GetString()
                    : null;

                var extOrderId = order.TryGetProperty("extOrderId", out var extOrderIdElement)
                    ? extOrderIdElement.GetString()
                    : null;

                var status = order.TryGetProperty("status", out var statusElement)
                    ? statusElement.GetString()
                    : null;

                if (string.IsNullOrWhiteSpace(orderId) && string.IsNullOrWhiteSpace(extOrderId))
                {
                    return BadRequest("Brak orderId oraz extOrderId w webhooku PayU.");
                }

                if (string.IsNullOrWhiteSpace(status))
                {
                    return BadRequest("Brak statusu płatności w webhooku PayU.");
                }

                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p =>
                        (!string.IsNullOrWhiteSpace(orderId) && p.PaymentProviderOrderId == orderId) ||
                        (!string.IsNullOrWhiteSpace(extOrderId) && p.ExternalOrderId == extOrderId));

                if (payment == null)
                {
                    return BadRequest("Nie znaleziono płatności dla orderId/extOrderId z PayU.");
                }

                _context.PaymentTransactions.Add(new PaymentTransaction
                {
                    PaymentId = payment.PaymentId,
                    ExternalTransactionId = orderId ?? extOrderId ?? "BRAK_ORDER_ID",
                    ProviderResponseCode = status,
                    ProviderResponseMessage = body,
                    CreatedAt = DateTime.Now
                });

                var payuStatus = status.ToUpperInvariant();

                if (payuStatus == "COMPLETED")
                {
                    await SetPaymentPaidWithoutSave(payment);
                }
                else if (payuStatus == "CANCELED" || payuStatus == "REJECTED" || payuStatus == "ERROR")
                {
                    await SetPaymentRejectedWithoutSave(payment);
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
        }

        [Authorize(Roles = "Administrator,Manager")]
        [HttpGet]
        public async Task<IActionResult> Confirm(int reservationId)
        {
            if (reservationId <= 0)
            {
                TempData["Error"] = "Niepoprawne ID rezerwacji.";
                return RedirectToAction("Go", "Reservation");
            }

            var payment = await _context.Payments
                .Include(p => p.Reservation)
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId);

            if (payment == null)
            {
                TempData["Error"] = "Nie znaleziono płatności.";
                return RedirectToAction("Go", "Reservation");
            }

            if (payment.Amount <= 0)
            {
                TempData["Error"] = "Kwota płatności musi być większa od zera.";
                return RedirectToAction("Go", "Reservation");
            }

            await SetPaymentPaid(payment, "MANUAL_CONFIRM", "Płatność została ręcznie potwierdzona w systemie.");

            TempData["Success"] = "Płatność została potwierdzona.";
            return RedirectToAction("Go", "Reservation");
        }

        private bool IsValidPayUSignature(string body)
        {
            var secondKey = _configuration["PayU:SecondKey"];

            if (string.IsNullOrWhiteSpace(secondKey))
            {
                return false;
            }

            var signatureHeader = Request.Headers["OpenPayu-Signature"].FirstOrDefault()
                                  ?? Request.Headers["OpenPayU-Signature"].FirstOrDefault()
                                  ?? Request.Headers["X-OpenPayU-Signature"].FirstOrDefault();

            if (string.IsNullOrWhiteSpace(signatureHeader))
            {
                return false;
            }

            var headerValues = ParsePayUSignatureHeader(signatureHeader);

            if (!headerValues.TryGetValue("signature", out var incomingSignature) ||
                string.IsNullOrWhiteSpace(incomingSignature))
            {
                return false;
            }

            if (headerValues.TryGetValue("algorithm", out var algorithm) &&
                !string.Equals(algorithm, "MD5", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var expectedSignature = CalculateMd5(body + secondKey);

            return string.Equals(
                expectedSignature,
                incomingSignature,
                StringComparison.OrdinalIgnoreCase
            );
        }

        private static Dictionary<string, string> ParsePayUSignatureHeader(string header)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var parts = header.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
                var keyValue = part.Split('=', 2, StringSplitOptions.TrimEntries);

                if (keyValue.Length == 2)
                {
                    result[keyValue[0]] = keyValue[1];
                }
            }

            return result;
        }

        private static string CalculateMd5(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var hashBytes = MD5.HashData(bytes);

            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        private async Task SetPaymentPaid(
            Payment payment,
            string responseCode,
            string responseMessage)
        {
            await SetPaymentPaidWithoutSave(payment);

            _context.PaymentTransactions.Add(new PaymentTransaction
            {
                PaymentId = payment.PaymentId,
                ExternalTransactionId = payment.PaymentProviderOrderId
                                        ?? payment.ExternalOrderId
                                        ?? "BRAK_ORDER_ID",
                ProviderResponseCode = responseCode,
                ProviderResponseMessage = responseMessage,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }

        private async Task SetPaymentRejected(
            Payment payment,
            string responseCode,
            string responseMessage)
        {
            await SetPaymentRejectedWithoutSave(payment);

            _context.PaymentTransactions.Add(new PaymentTransaction
            {
                PaymentId = payment.PaymentId,
                ExternalTransactionId = payment.PaymentProviderOrderId
                                        ?? payment.ExternalOrderId
                                        ?? "BRAK_ORDER_ID",
                ProviderResponseCode = responseCode,
                ProviderResponseMessage = responseMessage,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }

        private async Task SetPaymentPaidWithoutSave(Payment payment)
        {
            var paidStatus = await _context.PaymentStatuses
                .FirstOrDefaultAsync(s => s.Name == "Paid");

            if (paidStatus == null)
            {
                throw new InvalidOperationException("Brak statusu płatności Paid w bazie.");
            }

            payment.PaymentStatusId = paidStatus.PaymentStatusId;
            payment.PaidAt = DateTime.Now;
        }

        private async Task SetPaymentRejectedWithoutSave(Payment payment)
        {
            var rejectedStatus = await _context.PaymentStatuses
                .FirstOrDefaultAsync(s => s.Name == "Rejected");

            if (rejectedStatus == null)
            {
                throw new InvalidOperationException("Brak statusu płatności Rejected w bazie.");
            }

            payment.PaymentStatusId = rejectedStatus.PaymentStatusId;
            payment.PaidAt = null;
        }
    }
}

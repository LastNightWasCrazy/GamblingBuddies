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
        private readonly IConfiguration _configuration;

        public PaymentController(AppDbContext context, PayUService payUService, IConfiguration configuration)
        {
            _context = context;
            _payUService = payUService;
            _configuration = configuration;
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

            var publicUrl = _configuration["AppSettings:PublicUrl"]?.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(publicUrl))
            {
                TempData["Error"] = "Brak AppSettings:PublicUrl w appsettings.json.";
                return RedirectToAction("Go", "Reservation");
            }

            var continueUrl = $"{publicUrl}/Payment/Continue?reservationId={reservationId}";
            var notifyUrl = $"{publicUrl}/Payment/Notify";

            Console.WriteLine("PAYU CONTINUE URL: " + continueUrl);
            Console.WriteLine("PAYU NOTIFY URL: " + notifyUrl);

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

                if (string.IsNullOrEmpty(result.RedirectUri))
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
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId);

            if (payment == null)
            {
                TempData["Error"] = "Nie znaleziono płatności.";
                return RedirectToAction("Go", "Reservation");
            }

            TempData["Success"] = "Wrócono z PayU. Status płatności zostanie zaktualizowany po powiadomieniu z PayU.";

            return RedirectToAction("Go", "Reservation");
        }

        [HttpGet]
        public async Task<IActionResult> Reject(int reservationId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId);

            if (payment == null)
            {
                TempData["Error"] = "Nie znaleziono płatności.";
                return RedirectToAction("Go", "Reservation");
            }

            await SetPaymentRejected(
                payment,
                "PAYU_REJECT",
                "Płatność została odrzucona lub anulowana."
            );

            TempData["Error"] = "Płatność została odrzucona lub anulowana.";
            return RedirectToAction("Go", "Reservation");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Notify()
        {
            Console.WriteLine("========== PAYU NOTIFY PRZYSZŁO ==========");

            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            Console.WriteLine("PAYU BODY:");
            Console.WriteLine(body);

            if (string.IsNullOrWhiteSpace(body))
            {
                Console.WriteLine("PAYU BODY PUSTE");
                return BadRequest("Puste body webhooka PayU.");
            }

            JsonDocument document;

            try
            {
                document = JsonDocument.Parse(body);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd parsowania JSON:");
                Console.WriteLine(ex.Message);
                return BadRequest("Niepoprawny JSON z PayU.");
            }

            using (document)
            {
                if (!document.RootElement.TryGetProperty("order", out var order))
                {
                    Console.WriteLine("Brak pola order w JSON");
                    return BadRequest("Brak pola order w webhooku PayU.");
                }

                var orderId = order.TryGetProperty("orderId", out var orderIdElement)
                    ? orderIdElement.GetString()
                    : null;

                var status = order.TryGetProperty("status", out var statusElement)
                    ? statusElement.GetString()
                    : null;

                var extOrderId = order.TryGetProperty("extOrderId", out var extOrderIdElement)
                    ? extOrderIdElement.GetString()
                    : null;

                Console.WriteLine("PAYU orderId: " + orderId);
                Console.WriteLine("PAYU extOrderId: " + extOrderId);
                Console.WriteLine("PAYU status: " + status);

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
                        p.PaymentProviderOrderId == orderId ||
                        p.ExternalOrderId == extOrderId);

                if (payment == null)
                {
                    Console.WriteLine("NIE ZNALEZIONO PAYMENT PO orderId/extOrderId");

                    payment = await _context.Payments
                        .OrderByDescending(p => p.CreatedAt)
                        .FirstOrDefaultAsync(p => p.PaymentStatus.Name == "Pending");

                    if (payment == null)
                    {
                        Console.WriteLine("NIE ZNALEZIONO ŻADNEJ PENDING PŁATNOŚCI");
                        return Ok();
                    }

                    Console.WriteLine("UŻYWAM AWARYJNIE OSTATNIEJ PENDING PAYMENT ID: " + payment.PaymentId);
                }

                _context.PaymentTransactions.Add(new PaymentTransaction
                {
                    PaymentId = payment.PaymentId,
                    ExternalTransactionId = orderId ?? extOrderId ?? "BRAK_ORDER_ID",
                    ProviderResponseCode = status ?? "BRAK_STATUSU",
                    ProviderResponseMessage = body,
                    CreatedAt = DateTime.Now
                });

                var payuStatus = status?.ToUpper();

                if (payuStatus == "COMPLETED")
                {
                    var paidStatus = await _context.PaymentStatuses
                        .FirstOrDefaultAsync(s => s.Name == "Paid");

                    if (paidStatus != null)
                    {
                        payment.PaymentStatusId = paidStatus.PaymentStatusId;
                        payment.PaidAt = DateTime.Now;
                        Console.WriteLine("USTAWIONO PAID");
                    }
                }
                else if (
                    payuStatus == "CANCELED" ||
                    payuStatus == "REJECTED" ||
                    payuStatus == "ERROR")
                {
                    var rejectedStatus = await _context.PaymentStatuses
                        .FirstOrDefaultAsync(s => s.Name == "Rejected");

                    if (rejectedStatus != null)
                    {
                        payment.PaymentStatusId = rejectedStatus.PaymentStatusId;
                        payment.PaidAt = null;
                        Console.WriteLine("USTAWIONO REJECTED");
                    }
                    else
                    {
                        Console.WriteLine("BRAK STATUSU REJECTED W BAZIE");
                    }
                }
                else
                {
                    Console.WriteLine("STATUS NIE ZMIENIA PŁATNOŚCI: " + payuStatus);
                }

                await _context.SaveChangesAsync();

                Console.WriteLine("ZAPISANO ZMIANY W BAZIE");
                return Ok();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(int reservationId)
        {
            if (reservationId <= 0)
            {
                TempData["Error"] = "Niepoprawne ID rezerwacji.";
                return RedirectToAction("Go", "Reservation");
            }

            var payment = await _context.Payments
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

            if (payment.Reservation == null)
            {
                TempData["Error"] = "Płatność nie jest przypisana do rezerwacji.";
                return RedirectToAction("Go", "Reservation");
            }

            await SetPaymentPaid(payment, "MANUAL_CONFIRM", "Płatność została ręcznie potwierdzona w systemie.");

            TempData["Success"] = "Płatność została potwierdzona.";
            return RedirectToAction("Go", "Reservation");
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

            if (paidStatus != null)
            {
                payment.PaymentStatusId = paidStatus.PaymentStatusId;
                payment.PaidAt = DateTime.Now;
            }
        }

        private async Task SetPaymentRejectedWithoutSave(Payment payment)
        {
            var rejectedStatus = await _context.PaymentStatuses
                .FirstOrDefaultAsync(s => s.Name == "Rejected");

            if (rejectedStatus != null)
            {
                payment.PaymentStatusId = rejectedStatus.PaymentStatusId;
                payment.PaidAt = null;
            }
        }
    }
}
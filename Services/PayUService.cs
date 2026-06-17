using GamblingBuddies.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GamblingBuddies.Services.PayU
{
    public class PayUService : IPayUService
    {
        private readonly HttpClient _httpClient;
        private readonly PayUOptions _options;

        public PayUService(HttpClient httpClient, IOptions<PayUOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (string.IsNullOrWhiteSpace(_options.BaseUrl) ||
                string.IsNullOrWhiteSpace(_options.ClientId) ||
                string.IsNullOrWhiteSpace(_options.ClientSecret))
            {
                throw new InvalidOperationException("Brak konfiguracji PayU: BaseUrl, ClientId albo ClientSecret.");
            }

            var url = $"{_options.BaseUrl}/pl/standard/user/oauth/authorize";

            var body = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _options.ClientId),
                new KeyValuePair<string, string>("client_secret", _options.ClientSecret)
            });

            var response = await _httpClient.PostAsync(url, body);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Błąd pobierania tokenu PayU: " + json);
            }

            using var document = JsonDocument.Parse(json);

            return document.RootElement
                .GetProperty("access_token")
                .GetString()!;
        }

        public async Task<PayUCreateOrderResult> CreateOrderAsync(
            Payment payment,
            string continueUrl,
            string notifyUrl,
            string customerIp)
        {
            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }

            if (payment.PaymentId <= 0 || payment.ReservationId <= 0)
            {
                throw new ArgumentException("Płatność musi być zapisana w bazie przed wysłaniem do PayU.");
            }

            if (payment.Amount <= 0)
            {
                throw new ArgumentException("Kwota płatności musi być większa od zera.");
            }

            if (string.IsNullOrWhiteSpace(continueUrl))
            {
                throw new ArgumentException("Brak continueUrl dla PayU.");
            }

            if (string.IsNullOrWhiteSpace(notifyUrl))
            {
                throw new ArgumentException("Brak notifyUrl dla PayU.");
            }

            if (string.IsNullOrWhiteSpace(customerIp))
            {
                customerIp = "127.0.0.1";
            }

            if (string.IsNullOrWhiteSpace(_options.MerchantPosId))
            {
                throw new InvalidOperationException("Brak konfiguracji PayU: MerchantPosId.");
            }

            var token = await GetAccessTokenAsync();

            var amountInGrosze = (int)Math.Round(payment.Amount * 100m, MidpointRounding.AwayFromZero);

            var extOrderId = $"RES-{payment.ReservationId}-PAY-{payment.PaymentId}-{Guid.NewGuid()}";

            var order = new
            {
                notifyUrl = notifyUrl,
                continueUrl = continueUrl,
                customerIp = customerIp,
                merchantPosId = _options.MerchantPosId,
                description = $"Rezerwacja GamblingBuddies #{payment.ReservationId}",
                currencyCode = "PLN",
                totalAmount = amountInGrosze.ToString(),
                extOrderId = extOrderId,
                products = new[]
                {
                    new
                    {
                        name = $"Rezerwacja #{payment.ReservationId}",
                        unitPrice = amountInGrosze.ToString(),
                        quantity = "1"
                    }
                }
            };

            var json = JsonSerializer.Serialize(order);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_options.BaseUrl}/api/v2_1/orders"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.Found)
            {
                throw new Exception("Błąd tworzenia zamówienia PayU: " + responseBody);
            }

            var result = new PayUCreateOrderResult();

            if (response.StatusCode == HttpStatusCode.Found)
            {
                var redirectUriFromHeader = response.Headers.Location?.ToString();

                if (string.IsNullOrEmpty(redirectUriFromHeader))
                {
                    throw new Exception("PayU zwróciło 302, ale bez nagłówka Location.");
                }

                result.RedirectUri = redirectUriFromHeader;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(responseBody))
                {
                    throw new Exception("PayU zwróciło pustą odpowiedź.");
                }

                using var document = JsonDocument.Parse(responseBody);

                if (document.RootElement.TryGetProperty("orderId", out var orderId))
                {
                    result.OrderId = orderId.GetString();
                }

                if (document.RootElement.TryGetProperty("redirectUri", out var redirectUri))
                {
                    result.RedirectUri = redirectUri.GetString();
                }
            }

            payment.ExternalOrderId = extOrderId;
            payment.PaymentProviderOrderId = result.OrderId;
            payment.PaymentProvider = "PayU";

            return result;
        }
    }
}
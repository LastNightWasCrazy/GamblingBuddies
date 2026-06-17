using GamblingBuddies.Models;

namespace GamblingBuddies.Services.PayU
{
    public interface IPayUService
    {
        Task<PayUCreateOrderResult> CreateOrderAsync(
            Payment payment,
            string continueUrl,
            string notifyUrl,
            string customerIp);
    }
}

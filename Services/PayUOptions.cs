namespace GamblingBuddies.Services.PayU
{
    public class PayUOptions
    {
        public string BaseUrl { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string MerchantPosId { get; set; } = "";
    }
}
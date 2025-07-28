namespace Fsel.Shared.Models.ShareModels
{
    public class SendNotifyBuyBlindBoxModel
    {
        public int StatusCode { get; set; }
        public string? ErrorCode { get; set; }
        public string? ConfigType { get; set; }
        public string? ImagePath { get; set; }
        public int? Coin { get; set; }
    }
}

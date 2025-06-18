namespace Fsel.Ordering.Domain.Models.CommandModels.MarketPlacePremium
{
    public class RedeemProductPremiumCommandModel
    {
        public Guid ProductId { get; set; }
        public string? PhoneNumber { get; set; }
    }
}

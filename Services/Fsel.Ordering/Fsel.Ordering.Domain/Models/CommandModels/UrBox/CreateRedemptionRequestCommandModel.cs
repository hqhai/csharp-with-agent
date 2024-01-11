// Copyright (c) Atlantic. All rights reserveiytrd.

namespace Fsel.Ordering.Domain.Models.CommandModels.UrBox
{
    public class CreateRedemptionRequestCommandModel
    {
        public string? PhoneNumber { get; set; }
        public string? CityId { get; set; }
        public string? DistrictId { get; set; }
        public string? WardId { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        public IList<DataBuyCommandModel>? DataBuy { get; set; }
    }

    public class DataBuyCommandModel
    {
        public string? PriceId { get; set; }
        public string? Quantity { get; set; }
    }
}

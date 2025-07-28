// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UrBoxService.Models.Response
{
    using Fsel.Shared.Enums;

    public class GiftHistoryModel
    {
        public string? Id { get; set; }
        public string? GiftId { get; set; }
        public string? GiftName { get; set; }
        public string? Price { get; set; }
        public string? Content { get; set; }
        public string? Note { get; set; }
        public string? Expired { get; set; }
        public string? CodeImage { get; set; }
        public string? Code { get; set; }
        public string? Image { get; set; }
        public string? BrandTitle { get; set; }
        public string? BrandImage { get; set; }
        public string? Delivery { get; set; }
        public string? Pin { get; set; }
        public string? Serial { get; set; }
        public IList<string?>? Offices { get; set; }
        public EnumMarketPlaceType? MarketPlaceType { get; set; }
    }
}

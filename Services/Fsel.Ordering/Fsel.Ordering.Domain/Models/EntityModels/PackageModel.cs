// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class PackageModel
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public EnumPackageCode? Code { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public decimal PriceMonth { get; set; }
        public int MonthNumber { get; set; }
        public int MonthBonus { get; set; }
        public int DayBonus { get; set; }
        public int ReferToken { get; set; }
        public int BonusCoins { get; set; }
        public IList<string>? ImagePaths { get; set; }
        public string? EventDescription { get; set; }
        public string? Description { get; set; }
        public string? IncentivesWhenPurchasing { get; set; }
        public IList<EnumPackageSuggest>? Suggests { get; set; }
    }
}

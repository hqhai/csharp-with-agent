// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class PackageModel
    {
        public Guid Id { get; set; }
        public EnumPackageCode? Code { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public decimal PriceMonth { get; set; }
        public int MonthNumber { get; set; }
        public double MonthBonusNumber { get; set; }
        public string? IncentivesWhenPurchasing { get; set; }
        public EnumPackageSuggest? Suggest { get; set; }
        //public IList<PackageTranslationModel>? Translations { get; set; }
    }
}

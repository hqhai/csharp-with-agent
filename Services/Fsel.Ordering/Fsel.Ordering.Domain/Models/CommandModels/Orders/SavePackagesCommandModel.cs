// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders
{
    using Fsel.Shared.Enums;

    public class SavePackagesCommandModel
    {
        public IList<SavePackageCommandModel>? Packages { get; set; }
    }

    public class SavePackageCommandModel
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public decimal PriceMonth { get; set; }
        public double MonthBonusNumber { get; set; }
        public EnumPackageSuggest? Suggest { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders
{
    public class SavePackagesCommandModel
    {
        public IList<SavePackageCommandModel>? Packages { get; set; }
    }

    public class SavePackageCommandModel
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public decimal PriceMonth { get; set; }
        public int MonthNumber { get; set; }
        public string? Description { get; set; }
        public int ReferToken { get; set; }
        public int BonusCoins { get; set; }
    }
}

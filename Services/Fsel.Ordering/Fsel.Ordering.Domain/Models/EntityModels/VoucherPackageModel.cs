// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Domain.Entities.PackageConfigs;
    using Fsel.Ordering.Domain.Enums;

    public class VoucherPackageModel : BaseModel
    {
        public double Percentage { get; set; }
        public double DiscountedPrice { get; set; }
        public Guid PackageId { get; set; }
        public Guid VoucherId { get; set; }
        public double Price { get; set; }
        public EnumPackageCode? Code { get; set; }
        public IList<PackageConfig>? Description { get; set; }
        public PackageModel? Package { get; set; }
    }
}

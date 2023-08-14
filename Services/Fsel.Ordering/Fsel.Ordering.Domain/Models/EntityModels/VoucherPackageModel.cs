// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class VoucherPackageModel : BaseModel
    {
        public double Percentage { get; set; }
        public double DiscountedPrice { get; set; }
        public Guid PackageId { get; set; }
        public Guid VoucherId { get; set; }
        public double Price { get; set; }
        public PackageModel? Package { get; set; }
    }
}

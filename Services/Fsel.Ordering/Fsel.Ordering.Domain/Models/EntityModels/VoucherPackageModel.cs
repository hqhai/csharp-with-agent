// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System;

    public class VoucherPackageModel
    {
        public double Percentage { get; set; }

        public Guid PackageId { get; set; }
    }
}

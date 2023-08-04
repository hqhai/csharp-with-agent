// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.VoucherPackages
{
    using System;

    public class CreateVoucherPackageModel
    {
        public double Percentage { get; set; }

        public Guid PackageId { get; set; }
    }
}

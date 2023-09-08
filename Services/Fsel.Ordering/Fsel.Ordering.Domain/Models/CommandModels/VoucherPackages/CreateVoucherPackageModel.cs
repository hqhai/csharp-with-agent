// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.VoucherPackages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class CreateVoucherPackageModel
    {
        public double Percentage { get; set; }

        public Guid PackageId { get; set; }
    }
}

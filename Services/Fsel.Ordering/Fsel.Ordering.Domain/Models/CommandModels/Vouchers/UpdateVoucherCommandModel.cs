// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Vouchers
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.Models.CommandModels.VoucherPackages;
    using Fsel.Shared.Enums;

    public class UpdateVoucherCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? ContentFilePath { get; set; }

        public bool IsActive { get; set; }
        public EnumCustomerType CustomerType { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
        public IList<CreateVoucherPackageModel>? VoucherPackages { get; set; }
    }
}

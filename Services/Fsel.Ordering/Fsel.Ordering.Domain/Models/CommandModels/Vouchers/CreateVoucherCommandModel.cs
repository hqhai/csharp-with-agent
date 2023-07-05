// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Vouchers
{
    using System;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.Models.CommandModels.VoucherPackages;
    using Fsel.Shared.Enums;

    public class CreateVoucherCommandModel
    {
        public string? Name { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? ContentFilePath { get; set; }

        public IList<CreateVoucherPackageModel>? VoucherPackages { get; set; }
        public EnumCustomerType CustomerType { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
    }
}

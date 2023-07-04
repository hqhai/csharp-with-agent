// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Vouchers
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Ordering.Domain.Models.CommandModels.Packages;
    using Fsel.Ordering.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;

    public class UpdateVoucherCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? ContentFilePath { get; set; } 
        public bool IsGlobal { get; set; }
        public EnumCustomerType CustomerType { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public IList<VoucherPacketModel>? VoucherPackets { get; set; }
    }
}

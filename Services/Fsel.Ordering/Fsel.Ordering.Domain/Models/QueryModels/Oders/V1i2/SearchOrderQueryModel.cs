// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.Oders.V1i2
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchOrderQueryModel : BaseQueryModel
    {
        public EnumOrderStatus? Status { get; set; }
        public IList<Guid>? PackageIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public EnumPaymentRevenueType? RevenueType { get; set; }
    }
}

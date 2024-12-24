// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.Vouchers
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class SearchDetailHistoryVoucherAutoQueryModel : BaseQueryModel
    {
        public string? CodePrefix { get; set; }
        public bool? Status { get; set; }
        public DateTime? Day { get; set; }
    }
}

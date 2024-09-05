// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.Vouchers
{
    using Fsel.Core.Base.BaseModels;

    public class SearchHistoryVoucherQueryModel : BaseQueryModel
    {
        public bool? Status { get; set; }
        public DateTime? Day { get; set; }
    }
}

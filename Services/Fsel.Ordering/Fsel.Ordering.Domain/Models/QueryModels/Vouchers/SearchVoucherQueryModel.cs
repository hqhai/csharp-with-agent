// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.Vouchers
{
    using Fsel.Core.Base.BaseModels;

    public class SearchVoucherQueryModel : BaseQueryModel
    {
        public bool? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}

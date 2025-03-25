// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.Vouchers
{
    using Fsel.Core.Base.BaseModels;

    public class SearchVoucherByUserQueryModel : BaseQueryModel
    {
        /// <summary>
        /// False là sort theo ngày tạo, true là sắp xếp theo ngày hết hạn
        /// </summary>
        public bool Sort { get; set; }
    }
}

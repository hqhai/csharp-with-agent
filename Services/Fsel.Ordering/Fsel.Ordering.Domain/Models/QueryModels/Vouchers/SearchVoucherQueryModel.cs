// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.Vouchers
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchVoucherQueryModel : BaseQueryModel
    {
        public EnumVoucherSource Source { get; set; }
    }
}

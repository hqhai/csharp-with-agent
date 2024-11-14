// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.Products
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchProductQueryModel : BaseQueryModel
    {
        #region Dùng cho LMS Admin

        public string? Code { get; set; }
        public string? Name { get; set; }
        public EnumProductStatus? Status { get; set; }

        #endregion Dùng cho LMS Admin

        #region Dùng cho LMS

        public IList<Guid>? EventIds { get; set; }
        public bool? PopularOrLatest { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }

        #endregion Dùng cho LMS
    }
}

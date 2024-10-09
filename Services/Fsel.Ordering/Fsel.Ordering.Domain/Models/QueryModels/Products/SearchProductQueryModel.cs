// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.Products
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchProductQueryModel : BaseQueryModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public EnumProductStatus? Status { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.Oders
{
    using Fsel.Core.Base.BaseModels;

    public class SearchOderQueryModel : BaseQueryModel
    {
        public bool? Status { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.QueryModels.Oders
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchOrderQueryModel : BaseQueryModel
    {
        public EnumOrderStatus? Status { get; set; }
        public IList<Guid>? PackageIds { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

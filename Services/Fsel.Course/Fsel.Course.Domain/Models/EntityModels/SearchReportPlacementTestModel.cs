// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Models.ShareModels.EntityModels;

    public class SearchReportPlacementTestModel : OverallReportPlacementTestModel
    {
        public PagingItemsModel<PlacementTestReportModel>? PagingItems { get; set; }
    }
}

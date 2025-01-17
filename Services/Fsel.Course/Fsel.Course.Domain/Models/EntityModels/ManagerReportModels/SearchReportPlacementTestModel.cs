// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchReportPlacementTestModel : OverallReportPlacementTestModel
    {
        public PagingItemsModel<PlacementTestReportModel>? PagingItems { get; set; }
    }
}

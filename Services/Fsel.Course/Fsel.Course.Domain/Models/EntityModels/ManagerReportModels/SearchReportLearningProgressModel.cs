// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchReportLearningProgressModel : OverallReportLearningProgressModel
    {
        public PagingItemsModel<LearningProgressModel>? PagingItems { get; set; }
    }
}

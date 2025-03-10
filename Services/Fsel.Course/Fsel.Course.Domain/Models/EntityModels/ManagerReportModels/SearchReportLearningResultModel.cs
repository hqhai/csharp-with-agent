// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchReportLearningResultModel : OverallReportLearningResultModel
    {
        public PagingItemsModel<LearningResultModel>? PagingItems { get; set; }
    }
}

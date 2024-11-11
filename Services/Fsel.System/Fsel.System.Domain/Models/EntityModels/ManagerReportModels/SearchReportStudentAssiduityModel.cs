// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels.ManagerReportModels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchReportStudentAssiduityModel : OverallReportStudentAssiduityModel
    {
        public PagingItemsModel<StudentAssiduityModel>? PagingItems { get; set; }
    }
}

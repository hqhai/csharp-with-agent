// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Shared.Enums;

    public class SearchReportPlacementTestQueryModel : BaseSearchStudentReportQuery
    {
        public DateTime? StartDate { get; set; }
        public EnumCompletionStatus? Status { get; set; }
    }
}

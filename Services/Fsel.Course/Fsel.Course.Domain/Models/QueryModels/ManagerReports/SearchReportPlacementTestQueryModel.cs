// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Shared.Enums;

    public class SearchReportPlacementTestQueryModel : BaseSearchStudentReportQueryModel
    {
        public DateTime? StartDate { get; set; }
        public EnumCourseLevel? CurrentLevel { get; set; }
        public EnumCompletionStatus? Status { get; set; }
    }
}

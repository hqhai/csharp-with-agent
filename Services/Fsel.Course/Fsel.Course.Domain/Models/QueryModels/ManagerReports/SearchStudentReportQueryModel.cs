// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.QueryModels;

    public class SearchStudentReportQueryModel : SearchStudentSchoolQueryModel
    {
        public DateTime? StartDate { get; set; }
        public EnumCourseLevel? CurrentLevel { get; set; }
        public EnumOverallScore? OverallScore { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels.QueryModels;

    public class SearchReportPlacementTestQueryModel : SearchStudentSchoolQueryModel
    {
        public DateTime? StartDate { get; set; }
        public EnumCourseLevel? CurrentLevel { get; set; }
    }
}

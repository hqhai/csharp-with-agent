// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.QueryModels;

    public class SearchReportPlacementTestQueryModel : SearchStudentSchoolQueryModel
    {
        public DateTime? StartDate { get; set; }
        public string? ListCurrentLevel { get; set; }

        public IList<EnumCourseLevel>? CourseLevels
        {
            get
            {
                return ListCurrentLevel.ToList<EnumCourseLevel>();
            }
        }
    }
}

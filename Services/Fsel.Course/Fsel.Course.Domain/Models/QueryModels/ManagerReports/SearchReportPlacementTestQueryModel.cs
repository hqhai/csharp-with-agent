// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using System.Text.Json.Serialization;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.QueryModels;

    public class SearchReportPlacementTestQueryModel : SearchStudentSchoolQueryModel
    {
        public DateTime? StartDate { get; set; }

        [JsonIgnore]
        public IList<EnumCourseLevel>? CourseLevels
        {
            get
            {
                return ListCurrentLevel.ToList<EnumCourseLevel>();
            }
        }

        [JsonIgnore]
        public IList<EnumCompletionStatus>? CompletionStatuses
        {
            get
            {
                return ListCompletionStatus.ToList<EnumCompletionStatus>();
            }
        }
    }
}

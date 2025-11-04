// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using System.Text.Json.Serialization;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using Fsel.Shared.Models.ShareModels.QueryModels;

    public class SearchReportLearningResultQueryModel : SearchStudentSchoolQueryModel
    {
        [JsonIgnore]
        public IList<EnumOverallScore>? OverallScores
        {
            get
            {
                return ListOverallScore.ToList<EnumOverallScore>();
            }
        }

        public override EnumCourseType? CourseType { get; set; } = EnumCourseType.Academic;
        public bool? LearningStatus { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }
}

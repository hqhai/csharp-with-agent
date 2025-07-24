// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class SearchStudentSchoolQueryModel : BaseQueryModel
    {
        public string? ListSchoolGrade { get; set; }
        public string? ListSchoolClass { get; set; }
        public string? ListSchool { get; set; }
        public string? ListCourseLevel { get; set; }
        public string? ListDistrict { get; set; }
        public string? ListProvince { get; set; }
        public string? ListCompletionStatus { get; set; }
        public string? ListLearningStatus { get; set; }
        public string? ListOverallScore { get; set; }
        public string? ListCurrentLevel { get; set; }

        public IList<EnumCompletionStatus>? CompletionStatuses
        {
            get
            {
                return ListCompletionStatus.ToList<EnumCompletionStatus>();
            }
        }

        public IList<EnumLearningStatus>? LearningStatuses
        {
            get
            {
                return ListLearningStatus.ToList<EnumLearningStatus>();
            }
        }

        public IList<EnumOverallScore>? OverallScores
        {
            get
            {
                return ListOverallScore.ToList<EnumOverallScore>();
            }
        }

        public IList<EnumCourseLevel>? CurrentLevels
        {
            get
            {
                return ListCurrentLevel.ToList<EnumCourseLevel>();
            }
        }

        public IList<EnumCourseLevel>? CourseLevels
        {
            get
            {
                return ListCourseLevel.ToList<EnumCourseLevel>();
            }
        }

        public virtual EnumCourseType? CourseType { get; set; }
        public virtual bool? IsLearning { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

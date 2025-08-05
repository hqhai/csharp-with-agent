// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchStudentSchoolQueryModel : BaseQueryModel
    {
        public string? ListSchoolGrade { get; set; }
        public string? ListSchoolClass { get; set; }
        public string? ListSchool { get; set; }
        public string? ListDistrict { get; set; }
        public string? ListProvince { get; set; }
        public string? ListCompletionStatus { get; set; }
        public string? ListLearningStatus { get; set; }
        public string? ListOverallScore { get; set; }
        public string? ListCourseLevel { get; set; }
        public string? ListCurrentLevel { get; set; }
        public virtual EnumCourseType? CourseType { get; set; }
        public virtual bool? IsLearning { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

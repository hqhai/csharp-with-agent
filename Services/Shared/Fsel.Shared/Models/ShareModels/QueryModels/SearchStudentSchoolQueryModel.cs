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
        public string? ListCourseType { get; set; }
        public string? ListCourseLevel { get; set; }
        public string? ListDistrict { get; set; }
        public string? ListProvince { get; set; }

        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public virtual bool? IsLearning { get; set; }
        public EnumCompletionStatus? Status { get; set; }
        public EnumLearningStatus? LearningStatus { get; set; }
        public virtual EnumCourseType? CourseType { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

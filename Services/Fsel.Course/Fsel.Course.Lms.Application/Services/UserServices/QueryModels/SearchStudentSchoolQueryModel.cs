// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.QueryModels
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
        public string? ListLearningStatus { get; set; }
        public bool IsCheckDate { get; set; }
        public bool? IsLearning { get; set; }
        public EnumCompletionStatus? Status { get; set; }
        public EnumLearningStatus? LearningStatus { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? SubjectId { get; set; }
        public string? LevelIdStr { get; set; }
    }
}

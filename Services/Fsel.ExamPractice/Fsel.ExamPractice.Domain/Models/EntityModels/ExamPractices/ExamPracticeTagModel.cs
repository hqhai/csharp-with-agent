// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ExamPracticeTagModel
    {
        public EnumExamPracticeSubType SubType { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }
        public string? Description { get; set; }
    }
}

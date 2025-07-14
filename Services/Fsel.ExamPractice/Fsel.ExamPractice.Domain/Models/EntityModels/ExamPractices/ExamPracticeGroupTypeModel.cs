// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ExamPracticeGroupTypeModel
    {
        public EnumExamPracticeSubType? SubType { get; set; }
        public IList<EnumCourseSkill> CourseSkills { get; set; } = new List<EnumCourseSkill>();

        public string? SubTypeDescription
        {
            get
            {
                return SubType?.GetDescription();
            }
        }

        public IList<ExamPracticeGroupModel> ExamPracticeGroupModels { get; set; } = new List<ExamPracticeGroupModel>();
    }
}

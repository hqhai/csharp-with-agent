// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using System.Collections.Generic;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.Shared.Enums;

    public class PracticeTimeLimitRuleModel
    {
        public EnumExamPracticeType Type { get; set; }
        public EnumExamPracticeSubType? SubType { get; set; }
        public EnumCourseSkill? Skill { get; set; }
        public IList<EnumPracticeTimeLimitOption> TimeLimits { get; set; } = new List<EnumPracticeTimeLimitOption>();
    }
}

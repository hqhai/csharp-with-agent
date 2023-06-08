// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class LessonSkillScoreModel
    {
        public EnumTimeCodeType Type { get; set; }
        public EnumCourseSkill Skill { get; set; }
        public long TotalCount { get; set; }
        public long CorrectCount { get; set; }
    }
}

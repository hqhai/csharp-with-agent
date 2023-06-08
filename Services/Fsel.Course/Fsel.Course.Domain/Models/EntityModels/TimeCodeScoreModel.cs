// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;

    public class TimeCodeScoreModel
    {
        public EnumTimeCodeType Type { get; set; }
        public IList<LessonSkillScoreModel>? LessonSkillScores { get; set; }
    }
}

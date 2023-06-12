// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;

    public class TimeCodeScoreModel
    {
        public EnumTimeCodeType Type { get; set; }
        public double Percent { get; set; }
        public IList<LessonSkillScoreModel>? SkillScores { get; set; }
    }
}

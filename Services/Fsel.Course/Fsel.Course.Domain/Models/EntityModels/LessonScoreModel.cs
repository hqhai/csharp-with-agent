// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class LessonScoreModel
    {
        public IList<LessonSkillScoreModel>? LessonSkillScores { get; set; }
        public double Percent { get; set; }
        public IList<TimeCodeScoreModel>? TimeCodeScores { get; set; }
    }
}

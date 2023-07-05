// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public class LessonScoreModel
    {
        public IList<SkillScores>? SkillScores { get; set; }
        public double Percent { get; set; }
        public double TotalCount { get; set; }
        public double CorrectCount { get; set; }
        public IList<TimeCodeScoreModel>? TimeCodeScores { get; set; }
    }
}

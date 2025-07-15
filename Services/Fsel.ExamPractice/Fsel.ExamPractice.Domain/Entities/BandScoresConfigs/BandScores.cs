// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.BandScoresConfigs
{
    using Fsel.Shared.Enums;

    public class BandScores
    {
        public double Scores { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public string? CorrectAnswers { get; set; }
        public string? SkillLevel { get; set; }
        public string? Description { get; set; }
    }
}

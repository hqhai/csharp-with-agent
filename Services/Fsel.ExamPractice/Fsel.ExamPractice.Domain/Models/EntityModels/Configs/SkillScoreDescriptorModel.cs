// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.Configs
{
    public class SkillScoreDescriptorModel
    {
        public double MinInclusive { get; set; }
        public double MaxInclusive { get; set; }
        public string? CorrectAnswers { get; set; }
        public string? CourseSkill { get; set; }
        public string? SkillLevel { get; set; }
        public string? Level { get; set; }
        public string? Description { get; set; }
    }
}

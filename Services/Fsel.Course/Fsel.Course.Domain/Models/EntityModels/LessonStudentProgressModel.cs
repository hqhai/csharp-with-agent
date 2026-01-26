// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;

    public class LessonStudentProgressModel
    {
        public string? Type { get; set; }
        public Guid ObjectId { get; set; }
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public EnumResultStatus Status { get; set; }
        public double Percent { get; set; }
        public double CorrectPercent { get; set; }
        public double? Score { get; set; }
        public long TimeSpent { get; set; }
        public DateTime? LastVisited { get; set; }
        public string? ContentCompleted { get; set; }
        public int Visit { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
        public TestSkillScores? TestSkillScores { get; set; }
        public int DisplayOrder { get; set; }
        public Guid? LearningResultId { get; set; }
    }
}

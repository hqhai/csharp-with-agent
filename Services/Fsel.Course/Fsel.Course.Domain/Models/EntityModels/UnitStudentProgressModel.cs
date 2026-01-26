// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;

    public class UnitStudentProgressModel
    {
        public string? Type { get; set; }
        public Guid ObjectId { get; set; }
        public string? Name { get; set; }
        public EnumResultStatus Status { get; set; } = EnumResultStatus.Unfinished;
        public long TimeSpent { get; set; }
        public DateTime? LastVisited { get; set; }
        public double CorrectPercent { get; set; }
        public string? ContentProgress { get; set; }
        public int TotalLesson { get; set; }
        public int TotalSkill { get; set; }
        public int? DisplayOrder { get; set; }
        public IList<TestSkillScores> SkillScores { get; set; } = new List<TestSkillScores>();
        public double ProcessPercent { get; set; }
        public double Scores { get; set; }
        public int TotalContent { get; set; }
        public int TotalContentComplete { get; set; }
        public Guid ModuleId { get; set; }
        public int? CurrentLesson { get; set; }
    }
}

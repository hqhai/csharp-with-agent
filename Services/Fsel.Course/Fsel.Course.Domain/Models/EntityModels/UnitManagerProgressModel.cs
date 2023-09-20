// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;

    public class UnitManagerProgressModel
    {
        public string? Type { get; set; }
        public Guid ObjectId { get; set; }
        public string? Name { get; set; }
        public EnumResultStatus Status { get; set; }
        public long TimeSpent { get; set; }
        public DateTime LastVisited { get; set; }
        public double PercentObject { get; set; }
        public string? ContentProgress { get; set; }
        public int TotalLesson { get; set; }
        public int TotalSkill { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
        public double Percent { get; set; }
    }
}

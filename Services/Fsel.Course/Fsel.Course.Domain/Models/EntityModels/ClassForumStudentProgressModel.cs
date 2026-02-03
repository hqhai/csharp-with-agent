// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;

    public class ClassForumStudentProgressModel
    {
        public EnumResultStatus Status { get; set; }
        public long TimeSpent { get; set; }
        public DateTime? LastVisited { get; set; }
        public int Visit { get; set; }
        public int? DisplayOrder { get; set; }
        public Guid ClassForumId { get; set; }
        public Guid? ClassForumResultId { get; set; }
        public SkillScores? SkillScores { get; set; }
    }
}

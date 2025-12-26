// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class SectionGroup : Entity
    {
        /// <summary>
        /// Thời gian làm bài
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double ExecutionTime { get; set; }

        [NotMapped]
        public TimeSpan ExecutionTimeSpan
        {
            get { return TimeSpan.FromSeconds(ExecutionTime); }
        }

        public string? AudioPath { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public Skill? Skill { get; set; }
        public Guid? SkillId { get; set; }

        public ICollection<Section> Sections { get; set; } = new List<Section>();
        public ICollection<MockTestScore> MockTestScores { get; set; } = new List<MockTestScore>();
        public ICollection<MockTestSection> MockTestSections { get; set; } = new List<MockTestSection>();
        public ICollection<FinalTestSection> FinalTestSections { get; set; } = new List<FinalTestSection>();
        public ICollection<PlacementTestSection> PlacementTestSections { get; set; } = new List<PlacementTestSection>();
        public ICollection<SectionGroupResult> SectionGroupResults { get; set; } = new List<SectionGroupResult>();
    }
}

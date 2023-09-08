// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.ComponentModel.DataAnnotations;
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

        public EnumCourseSkill CourseSkill { get; set; }

        public ICollection<Section> Sections { get; set; } = new List<Section>();
        public ICollection<MockTestSection> MockTestSections { get; set; } = new List<MockTestSection>();
        public ICollection<FinalTestSection> FinalTestSections { get; set; } = new List<FinalTestSection>();
        public ICollection<PlacementTestSection> PlacementTestSections { get; set; } = new List<PlacementTestSection>();
    }
}

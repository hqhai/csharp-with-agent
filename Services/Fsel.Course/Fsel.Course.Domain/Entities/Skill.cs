// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.TestConfig;

    public class Skill : Entity
    {
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FilePath { get; set; }

        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        public Exercise? Exercise { get; set; }
        public SectionGroup? SectionGroup { get; set; }
        public ClassForum? ClassForum { get; set; }
        public HomeWork? HomeWork { get; set; }
        public LessonInstruction? LessonInstruction { get; set; }
        public ExtraPracticeExerciseResult? ExtraPracticeExerciseResult { get; set; }
        public ICollection<SkillLevel> SkillLevels { get; set; } = new List<SkillLevel>();
        public ICollection<Section>? Sections { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.FlowConfigs;

    public class Level : Entity
    {
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        public int LevelOrder { get; set; }

        public Guid ProgramId { get; set; }

        public Category? Category { get; set; }

        public ICollection<SkillLevel> SkillLevels { get; set; } = new List<SkillLevel>();
        public ICollection<StepFlow> StepFlows { get; set; } = new List<StepFlow>();
    }
}

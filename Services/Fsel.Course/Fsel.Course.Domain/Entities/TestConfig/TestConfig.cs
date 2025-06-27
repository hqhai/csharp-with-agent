// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfig
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class TestConfig : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(150, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(150, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        public bool IsActive { get; set; }
        public Guid? ProgramId { get; set; }
        public Category? Program { get; set; }
        public Guid? LevelId { get; set; }
        public Level? Level { get; set; }
        public ICollection<TestConfigSection>? TestConfigSections { get; set; }
    }
}

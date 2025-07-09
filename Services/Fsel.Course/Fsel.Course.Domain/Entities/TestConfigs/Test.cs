// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfigs
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Attributes;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class Test : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [RegexValid(Regex = @"^[a-zA-Z0-9_ ]{1,150}$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(150, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [RegexValid(Regex = @"^[a-zA-Z0-9_]{1,150}$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        [MaxLength(150, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        public bool IsArchive { get; set; }

        public Guid ProgramId { get; set; }
        public Category? Program { get; set; }
        public Guid LevelId { get; set; }
        public Level? Level { get; set; }
        public ICollection<TestSection> TestSections { get; set; } = new List<TestSection>();
        public ICollection<CategoryTestBank> CategoryTestBanks { get; set; } = new List<CategoryTestBank>();
    }
}

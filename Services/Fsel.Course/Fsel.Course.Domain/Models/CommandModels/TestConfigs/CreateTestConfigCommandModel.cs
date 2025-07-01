// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.TestConfigs
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Models.CommandModels.TestConfigSections;

    public class CreateTestConfigCommandModel
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(150, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(150, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? LevelId { get; set; }
        public bool IsActive { get; set; }
        public IList<CreateTestConfigSectionCommandModel>? TestConfigSections { get; set; }

    }
}

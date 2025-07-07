// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Tests
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Models.CommandModels.TestSections;

    public class CreateTestCommandModel
    {
        [MaxLength(150, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [MaxLength(150, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        public Guid? ProgramId { get; set; }
        public Guid? LevelId { get; set; }
        public IList<CreateTestSectionCommandModel> TestSections { get; set; } = new List<CreateTestSectionCommandModel>();
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.TestConfigs
{
    using System.Collections.Generic;
    using Fsel.Course.Domain.Models.CommandModels.TestConfigSections;

    public class CreateTestConfigCommandModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public Guid ProgramId { get; set; }
        public Guid LevelId { get; set; }
        public bool IsActive { get; set; }
        public IList<CreateTestConfigSectionCommandModel>? TestConfigSections { get; set; }

    }
}

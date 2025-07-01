// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.TestConfigs
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.CommandModels.TestConfigSections;

    public class UpdateTestConfigCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public Guid ProgramId { get; set; }
        public Guid LevelId { get; set; }
        public bool IsActive { get; set; }
        public IList<UpdateTestConfigSectionCommandModel>? TestConfigSections { get; set; }
    }
}

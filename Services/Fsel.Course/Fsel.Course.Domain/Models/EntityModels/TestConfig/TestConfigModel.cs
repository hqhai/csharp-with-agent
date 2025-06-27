// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestConfig
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class TestConfigModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; }
        public double TotalScore { get; set; }
        public double ExecutionTime { get; set; }
        public Guid? ProgramId { get; set; }
        public string? ProgramName { get; set; }

        public Guid? LevelId { get; set; }
        public string? LevelName { get; set; }

        public EnumTestLayoutType LayoutType { get; set; }

        public IList<string?>? SkillLevels { get; set; }
        public IList<TestConfigSectionModel>? TestConfigSectionModels { get; set; }
    }
}

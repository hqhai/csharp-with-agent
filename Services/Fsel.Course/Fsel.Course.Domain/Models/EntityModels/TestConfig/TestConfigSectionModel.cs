// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestConfig
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;

    public class TestConfigSectionModel : BaseModel
    {
        public string? Name { get; set; }
        public int TargetWord { get; set; }
        public int DisplayOrder { get; set; }
        public double? ExecutionTime { get; set; }
        public EnumTestLayoutType LayoutType { get; set; }
        public double? TotalScore { get; set; }
        public Skill? Skill { get; set; }
        public object? Config { get; set; }
        public Guid? ParentId { get; set; }
        public IList<TestConfigSectionModel>? Children { get; set; }
    }
}

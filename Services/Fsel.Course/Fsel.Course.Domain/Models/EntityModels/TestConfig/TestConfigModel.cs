// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestConfig
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfig;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;

    public class TestConfigModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; }
        public long TotalQuestion { get; set; }
        public double ExecutionTime { get; set; }
        public CategoryModel? Program { get; set; }
        public IList<SkillModel>? Skills { get; set; }
        public IList<TestLayoutModel>? TestLayouts { get; set; }
    }
}

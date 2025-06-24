// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestConfig
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfig;

    public class TestConfigModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; }
        public long TotalQuestion { get; set; }
        public double ExecutionTime { get; set; }
        public Category? Program { get; set; }
        public IList<Skill>? Skills { get; set; }
        public IList<TestLayout>? TestLayouts { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfig
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class TestLayout : Entity
    {
        public EnumTestLayoutName Name { get; set; }
        public double? TotalScore { get; set; }
        public string? ExcutionTime { get; set; }
        public Section? Section { get; set; }
    }
}

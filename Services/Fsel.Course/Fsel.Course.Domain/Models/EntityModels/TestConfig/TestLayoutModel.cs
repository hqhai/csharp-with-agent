// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestConfig
{
    using Fsel.Shared.Enums;

    public class TestLayoutModel
    {
        public EnumTestLayoutName Name { get; set; }
        public double? TotalScore { get; set; }
        public string? ExcutionTime { get; set; }
        public SectionModel? Section { get; set; }
    }
}

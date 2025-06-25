// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.TestLayouts
{
    using Fsel.Course.Domain.Models.CommandModels.Sections;
    using Fsel.Shared.Enums;

    public class CreateTestLayoutCommandModel
    {
        public EnumTestLayoutName Name { get; set; }
        public double? TotalScore { get; set; }
        public string? ExcutionTime { get; set; }
        public CreateSectionCommandModel? Section { get; set; }
    }
}

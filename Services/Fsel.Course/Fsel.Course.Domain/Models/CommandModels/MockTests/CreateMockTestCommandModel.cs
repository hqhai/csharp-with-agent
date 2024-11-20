// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.MockTests
{
    using System.Collections.Generic;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.SectionGroups;

    public class CreateMockTestCommandModel
    {
        public string? Name { get; set; }
        public double Version { get; set; }
        public EnumMockTestType MockTestType { get; set; }
        public IList<CreateSectionGroupCommandModel>? SectionGroups { get; set; }
    }
}

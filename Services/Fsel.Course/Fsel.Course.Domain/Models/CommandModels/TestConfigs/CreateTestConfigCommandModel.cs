// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.TestConfig
{
    using System.Collections.Generic;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.Categories;
    using Fsel.Course.Domain.Models.CommandModels.SectionGroups;
    using Fsel.Course.Domain.Models.CommandModels.Skills;
    using Fsel.Course.Domain.Models.CommandModels.TestLayouts;

    public class CreateTestConfigCommandModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public Guid ProgramId { get; set; }
        public IList<Guid> SkillIds { get; set; }
        public CreateTestLayoutCommandModel TestLayout { get; set; }
    }
}

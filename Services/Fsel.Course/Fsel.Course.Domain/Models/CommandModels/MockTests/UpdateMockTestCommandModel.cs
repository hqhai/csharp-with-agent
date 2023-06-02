// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.MockTests
{
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.SectionGroups;
    using Fsel.Course.Domain.Models.CommandModels.Sections;
    using Fsel.Shared.Enums;

    public class UpdateMockTestCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public EnumCourseType CourseType { get; set; }

        public EnumMockTestType MockTestType { get; set; }

        public IList<CreateSectionGroupCommandModel>? SectionGroups { get; set; }
    }
}

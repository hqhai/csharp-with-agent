// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.MockTests
{
    using System.Collections.Generic;
    using Fsel.Shared.Enums;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.SectionGroups;

    public class CreateMockTestCommandModel
    {
        public string? Name { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseType CourseType { get; set; }

        public EnumMockTestType MockTestType { get; set; }

        public IList<CreateSectionGroupCommandModel>? SectionGroups { get; set; }
    }
}

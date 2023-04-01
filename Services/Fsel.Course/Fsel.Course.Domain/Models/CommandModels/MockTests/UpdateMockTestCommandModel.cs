// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.MockTests
{
    using System.Collections.Generic;
    using Fsel.Common.Enums;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;

    public class UpdateMockTestCommandModel
    {
        public string? Name { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseType CourseType { get; set; }

        public EnumMockTestType MockTestType { get; set; }

        public IList<CourseUnitMockTest>? CourseUnitMockTests { get; set; }

        public IList<UnitSkillMockTest>? UnitSkillMockTests { get; set; }
    }
}

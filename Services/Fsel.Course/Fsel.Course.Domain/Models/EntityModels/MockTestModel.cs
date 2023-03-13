// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;

    public class MockTestModel : BaseEntityModel
    {
        public string? Name { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseType CourseType { get; set; }

        public EnumMockTestType MockTestType { get; set; }

        public List<CourseUnitMockTest>? CourseUnitMockTests { get; set; }

        public List<UnitSkillMockTest>? UnitSkillMockTests { get; set; }
    }
}

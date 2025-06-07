// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class MockTestSearchModel : BaseModel
    {
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumMockTestType MockTestType { get; set; }
        public double Version { get; set; }
        public IList<EnumCourseSkill>? Skills { get; set; }
        public IList<string?>? SkillNames { get; set; }
        public IList<Guid>? SkillIds { get; set; }
    }
}

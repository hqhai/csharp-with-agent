// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class MockTestResultSearchModel : BaseModel
    {
        public EnumMockTestType Type { get; set; }
        public Guid? UnitId { get; set; }
        public string? UnitName { get; set; }
        public Guid CourseId { get; set; }
        public Guid MockTestId { get; set; }
        public string? CourseName { get; set; }
        public string? PostArea { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }
    }
}

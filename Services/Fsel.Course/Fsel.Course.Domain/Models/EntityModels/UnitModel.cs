// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class UnitModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
        public IList<LessonModel>? Lessons { get; set; }
        public MockTestModel? SkillMockTest { get; set; }
        public UnitResultModel? UnitResult { get; set; }
    }
}

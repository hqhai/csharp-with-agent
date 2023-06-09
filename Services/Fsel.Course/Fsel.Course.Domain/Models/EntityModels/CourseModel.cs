// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class CourseModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? InstructionContent { get; set; }

        public EnumCourseStatus Status { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public EnumCourseType CourseType { get; set; }

        public IList<CourseUnitMockTestModel>? CourseUnitMockTests { get; set; }
        public IList<CourseTeacherModel>? CourseTeachers { get; set; }
        public CourseClassModel? CourseClass { get; set; }
    }
}

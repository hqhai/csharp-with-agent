// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;
using Fsel.Course.Domain.Models.CommandModels.CourseTeachers;
using Fsel.Course.Domain.Models.CommandModels.CourseUnitMockTests;

namespace Fsel.Course.Domain.Models.CommandModels.Courses
{
    public class CreateCourseCommandModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public IList<CreateCourseUnitMockTestCommandModel>? CourseUnitMockTests { get; set; }

        public IList<CreateCourseTeacherCommandModel>? CourseTeachers { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Shared.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class CourseSearchModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? InstructionContent { get; set; }

        public EnumCourseStatus Status { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public EnumCourseType CourseType { get; set; }

        public IList<Guid>? TeacherIds { get; set; }
        public int DurationMonth { get; set; }

        public int EnrollmentWeek { get; set; }

        public IList<string>? TeacherNames { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CourseSearchModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? InstructionContent { get; set; }

        public EnumCourseStatus Status { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public EnumCourseType CourseType { get; set; }

        public IList<Guid>? TeacherIds { get; set; }

        public IList<string>? TeacherNames { get; set; }

        public int? DurationMonth { get; set; }

        public int? EnrollmentWeek { get; set; }

        public Guid? LevelId { get; set; }

        public string? LevelName { get; set; }

        public Guid OriginalId { get; set; }

        public int Version { get; set; }

        public Guid? ProgramId { get; set; }
        public string? Program { get; set; }
        public Guid? SubjectId { get; set; }
        public string? Subject { get; set; }
    }
}

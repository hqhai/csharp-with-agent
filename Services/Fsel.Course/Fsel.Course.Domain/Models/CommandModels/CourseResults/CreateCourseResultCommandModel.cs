// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.CourseResults
{
    using System;
    using Fsel.Course.Domain.Enums;

    public class CreateCourseResultCommandModel
    {
        public int Result { get; set; }

        public EnumCourseStatus Status { get; set; }
        public Guid CourseId { get; set; }

        public Guid StudentId { get; set; }
    }
}

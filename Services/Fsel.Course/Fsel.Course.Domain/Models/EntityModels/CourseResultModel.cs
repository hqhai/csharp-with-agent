// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Course.Domain.Enums;

    public class CourseResultModel
    {
        public int Result { get; set; }

        public EnumCourseStatus Status { get; set; }
        public Guid CourseId { get; set; }

        public Guid StudentId { get; set; }
    }
}

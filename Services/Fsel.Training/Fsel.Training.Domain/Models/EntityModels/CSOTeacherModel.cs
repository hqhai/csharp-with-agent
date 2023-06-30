// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class CSOTeacherModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public int CountClass { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
    }
}

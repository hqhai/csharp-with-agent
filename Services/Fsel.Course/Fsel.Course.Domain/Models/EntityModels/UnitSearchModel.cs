// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;

    public class UnitSearchModel : BaseModel
    {
        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public bool IsActive { get; set; }

        public Guid? TeacherId { get; set; }

        public string? TeacherName { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}

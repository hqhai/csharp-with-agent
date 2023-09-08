// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Shared.Enums;
    using Fsel.Core.Base.BaseModels;

    public class UnitSearchModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public bool IsActive { get; set; }

        public IList<Guid>? TeacherIds { get; set; }

        public IList<string>? TeacherNames { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}

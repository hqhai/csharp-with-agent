// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Entities;

    public class StudentModel : BaseModel
    {
        public string? Membership { get; set; }

        public string? Occupation { get; set; }

        public string? School { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid ClassId { get; set; }

        public Human? Human { get; set; }

        public Guid HumanId { get; set; }
    }
}

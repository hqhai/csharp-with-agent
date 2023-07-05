// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Shared.Enums;
    using Fsel.Core.Base.BaseModels;

    public class StudentModel : BaseModel
    {
        public Guid? PackageId { get; set; }
        public string? Occupation { get; set; }
        public string? School { get; set; }
        public HumanProfileModel? Human { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid? ClassId { get; set; }
        public Guid HumanId { get; set; }
        public string? UserId { get; set; }
        public ParentProfileModel? Parent { get; set; }
    }
}

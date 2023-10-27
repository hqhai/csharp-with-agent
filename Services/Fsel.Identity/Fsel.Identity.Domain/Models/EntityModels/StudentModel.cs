// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentModel : BaseModel
    {
        public Guid? PackageId { get; set; }
        public string? Occupation { get; set; }
        public string? School { get; set; }
        public HumanProfileModel? Human { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public string? Membership { get; set; }
        public Guid? ClassId { get; set; }
        public string? CodeClass { get; set; }
        public long NumberOfToken { get; set; }
        public ParentProfileModel? Parent { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Shared.Enums;

    public class UserStudentModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? Code { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Membership { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public string? CodeClass { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Occupation { get; set; }
        public string? School { get; set; }
        public EnumGender? Gender { get; set; }
        public Guid? PackageId { get; set; }
        public ParentInfoModel? Parent { get; set; }
    }
}

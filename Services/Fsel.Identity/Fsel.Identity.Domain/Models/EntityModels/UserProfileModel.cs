// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Shared.Enums;

    public class UserProfileModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? School { get; set; }
        public EnumGender? Gender { get; set; }
        public bool? PhoneNumberConfirmed { get; set; }
        public string? Email { get; set; }
        public string? AvatarPath { get; set; }
        public Guid? ClassId { get; set; }
        public string? CodeClass { get; set; }
        public IList<string>? Roles { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid? PackageId { get; set; }
        public string? Membership { get; set; }
        public string? Occupation { get; set; }
        public string? PassportPath { get; set; }
        public string? UniversityDegreePath { get; set; }
        public string? CertificationPath { get; set; }
        public string? PoliceClearancePath { get; set; }
        public IList<TeacherBankAccountModel>? TeacherBankAccounts { get; set; }
        public IList<StudentProfileModel>? Students { get; set; }
        public ParentProfileModel? Parent { get; set; }
    }
}

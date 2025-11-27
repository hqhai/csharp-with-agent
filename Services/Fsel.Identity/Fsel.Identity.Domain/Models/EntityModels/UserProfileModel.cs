// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;
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
        public string? SchoolName { get; set; }
        public EnumGender? Gender { get; set; }
        public bool? PhoneNumberConfirmed { get; set; }
        public string? Email { get; set; }

        private string? _avatarPath;

        public string? AvatarPath
        {
            set { _avatarPath = value; }
            get { return _avatarPath.AddS3BaseUrl(); }
        }

        public bool IsEnabledExtra { get; set; }
        public Guid? ClassId { get; set; }
        public string? CodeClass { get; set; }
        public IList<string>? Roles { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid? PackageId { get; set; }
        public int CountPTResult { get; set; }
        public string? Membership { get; set; }
        public string? Occupation { get; set; }

        private string? _passportPath;

        public string? PassportPath
        {
            set { _passportPath = value; }
            get { return _passportPath.AddS3BaseUrl(); }
        }

        private string? _universityDegreePath;

        public string? UniversityDegreePath
        {
            set { _universityDegreePath = value; }
            get { return _universityDegreePath.AddS3BaseUrl(); }
        }

        private string? _certificationPath;

        public string? CertificationPath
        {
            set { _certificationPath = value; }
            get { return _certificationPath.AddS3BaseUrl(); }
        }

        private string? _policeClearancePath;

        public string? PoliceClearancePath
        {
            set { _policeClearancePath = value; }
            get { return _policeClearancePath.AddS3BaseUrl(); }
        }

        public IList<TeacherBankAccountModel>? TeacherBankAccounts { get; set; }
        public IList<StudentProfileModel>? Students { get; set; }
        public ParentProfileModel? Parent { get; set; }
        public SenderModel? Sender { get; set; }

        public EnumStatusStudentCampus? StatusStudentCampus { get; set; }
    }
}

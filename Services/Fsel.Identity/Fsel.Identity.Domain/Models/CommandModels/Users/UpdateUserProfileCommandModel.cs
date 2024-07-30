// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    using Fsel.Identity.Domain.Models.CommandModels.Parents;
    using Fsel.Identity.Domain.Models.CommandModels.Students;
    using Fsel.Identity.Domain.Models.CommandModels.TeacherBankAccount;
    using Fsel.Shared.Enums;

    public class UpdateUserProfileCommandModel
    {
        public string? AvatarPath { get; set; }
        public string? FullName { get; set; }
        public DateTime Birthday { get; set; }
        public EnumGender? Gender { get; set; }
        public string? School { get; set; }
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public string? PassportPath { get; set; }
        public string? UniversityDegreePath { get; set; }
        public string? CertificationPath { get; set; }
        public string? PoliceClearancePath { get; set; }

        public string? PhoneNumber { get; set; }

        public IList<UpdateStudentProfileCommandModel>? Students { get; set; }
        public UpdateParentCommandModel? Parent { get; set; }
        public UpdateTeacherBankAccountCommandModel? TeacherBankAccount { get; set; }
    }
}

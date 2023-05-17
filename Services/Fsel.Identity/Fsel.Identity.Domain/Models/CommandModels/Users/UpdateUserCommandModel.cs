// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Models.CommandModels.TeacherBankAccount;

    public class UpdateUserCommandModel : BaseCommandModel
    {
        public string? AvatarPath { get; set; }
        public DateTime? Birthday { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? PassportPath { get; set; }
        public string? UniversityDegreePath { get; set; }
        public string? CertificationPath { get; set; }
        public string? PoliceClearancePath { get; set; }
        public UpdateTeacherBankAccountCommandModel? TeacherBankAccount { get; set; }
    }
}

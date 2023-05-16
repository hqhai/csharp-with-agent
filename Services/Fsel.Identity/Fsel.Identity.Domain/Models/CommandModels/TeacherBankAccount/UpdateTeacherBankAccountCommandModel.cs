// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.TeacherBankAccount
{
    using Fsel.Shared.Enums;

    public class UpdateTeacherBankAccountCommandModel
    {
        public string? BankAccountName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }
        public EnumStatusBank Status { get; set; }
    }
}

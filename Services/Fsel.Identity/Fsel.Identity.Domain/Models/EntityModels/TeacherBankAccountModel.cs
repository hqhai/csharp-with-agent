// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;

    public class TeacherBankAccountModel
    {
        public string? BankAccountName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }
        public string? BankBranch { get; set; }
        public Guid TeacherId { get; set; }
    }
}

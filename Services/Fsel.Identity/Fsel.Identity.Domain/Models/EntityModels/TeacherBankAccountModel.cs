// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Shared.Enums;

    public class TeacherBankAccountModel
    {
        public string? BankAccountName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }
        public EnumStatusBank Status { get; set; }
        public Guid TeacherId { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    public class CreateUserAndOrderByAdminCommandModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? School { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? ReferralCode { get; set; }
        public int MonthNumber { get; set; }
        public bool IsRevenue { get; set; }
        public bool IsSendMail { get; set; }
        public string? VoucherCode { get; set; }
        public string? EventCode { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    public class HistoryVoucherModel
    {
        public string? StudentCode { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? VoucherCode { get; set; }
        public string? OrderCode { get; set; }
        public DateTime? DayUsed { get; set; }
        public bool Status { get; set; }
        public string? UsageStatus { get; set; }
        public Guid VoucherId { get; set; }
        public Guid? UserId { get; set; }
    }
}

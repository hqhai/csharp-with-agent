// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class HistoryVoucherModel
    {
        public string? StudentCode { get; set; }
        public string? Email { get; set; }
        public string? VoucherCode { get; set; }
        public string? OrderCode { get; set; }
        public DateTime DayUsed { get; set; }
        public EnumOrderStatus Status { get; set; }
        public Guid VoucherId { get; set; }
        public Guid UserId { get; set; }
    }
}

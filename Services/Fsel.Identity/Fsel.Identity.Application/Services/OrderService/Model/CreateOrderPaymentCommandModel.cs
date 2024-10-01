// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    using System;

    public class CreateOrderPaymentCommandModel
    {
        public Guid UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int MonthNumber { get; set; }
        public bool IsRevenue { get; set; }
        public bool IsSendMail { get; set; }
        public string? VoucherCode { get; set; }
        public string? EventCode { get; set; }
    }
}

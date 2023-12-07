// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders
{
    using Fsel.Shared.Enums;

    public class PaymentCommandModel
    {
        public string? OrderCode { get; set; }
        public EnumVNPAYPaymentType? VNPAYPaymentType { get; set; }
    }
}

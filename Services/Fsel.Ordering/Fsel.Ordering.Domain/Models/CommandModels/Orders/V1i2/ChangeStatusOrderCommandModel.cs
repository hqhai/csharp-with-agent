// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2
{
    using Fsel.Shared.Enums;

    public class ChangeStatusOrderCommandModel
    {
        public IList<Guid>? OrderIds { get; set; }
        public EnumOrderStatus Status { get; set; }
        public EnumPaymentRevenueType? RevenueType { get; set; }
    }
}

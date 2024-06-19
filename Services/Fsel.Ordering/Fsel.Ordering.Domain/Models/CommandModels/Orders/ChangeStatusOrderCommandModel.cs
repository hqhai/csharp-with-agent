// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders
{
    using System;
    using Fsel.Shared.Enums;

    public class ChangeStatusOrderCommandModel
    {
        public Guid OrderId { get; set; }
        public EnumOrderStatus OrderStatus { get; set; }
        public EnumOrderTransactionType? Type { get; set; }
        public string? Receipt { get; set; }
    }
}

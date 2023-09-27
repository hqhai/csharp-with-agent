// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    using System;
    using Fsel.Ordering.Domain.Enums;

    public class ChangeStatusOrderCommandModel
    {
        public Guid OrderId { get; set; }
        public Guid? PackageId { get; set; }
        public EnumOrderStatus OrderStatus { get; set; }
    }
}

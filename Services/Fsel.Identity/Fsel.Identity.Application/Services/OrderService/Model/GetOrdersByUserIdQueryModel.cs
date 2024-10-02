// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    using System;
    using Fsel.Shared.Enums;

    public class GetOrdersByUserIdQueryModel
    {
        public Guid? UserId { get; set; }
        public EnumOrderStatus? Status { get; set; }
    }
}

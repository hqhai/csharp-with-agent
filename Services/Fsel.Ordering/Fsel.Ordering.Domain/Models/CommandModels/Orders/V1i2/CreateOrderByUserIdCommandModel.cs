// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2
{
    using System;

    public class CreateOrderByUserIdCommandModel : CreateOrderCommandModel
    {
        public Guid UserId { get; set; }
    }
}

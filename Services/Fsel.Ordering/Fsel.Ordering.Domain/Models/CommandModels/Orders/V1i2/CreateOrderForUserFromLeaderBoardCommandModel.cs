// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Orders.V1i2
{
    using System;

    public class CreateOrderForUserFromLeaderBoardCommandModel : CreateOrderCommandModel
    {
        public Guid UserId { get; set; }
        public int Month { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }
}

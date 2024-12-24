// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Products
{
    using System;

    public class RedeemProductCommandModel
    {
        public Guid ProductId { get; set; }
        public string? PhoneNumber { get; set; }
    }
}

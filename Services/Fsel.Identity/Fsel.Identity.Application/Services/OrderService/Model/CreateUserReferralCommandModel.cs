// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    public class CreateUserReferralCommandModel
    {
        public Guid ReceiverId { get; set; }
        public string? ReferralCode { get; set; }
    }
}

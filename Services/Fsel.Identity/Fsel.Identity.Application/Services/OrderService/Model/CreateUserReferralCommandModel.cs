// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    public class CreateUserReferralCommandModel
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
    }
}

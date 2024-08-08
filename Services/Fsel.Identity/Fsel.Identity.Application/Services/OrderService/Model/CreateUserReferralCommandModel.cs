// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    using Fsel.Shared.Enums;

    public class CreateUserReferralCommandModel
    {
        public Guid? ReceiverId { get; set; }
        public string? ReferralCode { get; set; }
        public EnumUserReferralType UserReferralType { get; set; }
    }
}

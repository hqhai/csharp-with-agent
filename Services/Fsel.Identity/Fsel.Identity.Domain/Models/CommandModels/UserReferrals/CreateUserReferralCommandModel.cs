// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.UserReferrals
{
    public class CreateUserReferralCommandModel
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.OrderService.Model
{
    using Fsel.Core.Base.BaseModels;

    public class UserReferralModel : BaseModel
    {
        public int IndexNumber { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public int CountSender { get; set; }
        public int CountReceiver { get; set; }
    }
}

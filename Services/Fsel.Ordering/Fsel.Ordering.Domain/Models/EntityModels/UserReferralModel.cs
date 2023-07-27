// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class UserReferralModel : BaseModel
    {
        public int IndexNumber { get; set; }

        public Guid SenderId { get; set; }

        public Guid ReceiverId { get; set; }
    }
}

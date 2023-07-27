// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using Fsel.Core.Entities;

    public class UserReferral : Entity
    {
        public int IndexNumber { get; set; }

        public Guid SenderId { get; set; }

        public Guid ReceiverId { get; set; }
    }
}

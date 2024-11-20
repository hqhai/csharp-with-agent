// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System.ComponentModel;
    using Fsel.Core.Entities;

    public class UserVoucherLock : Entity
    {
        public int Count { get; set; }

        [DefaultValue(false)]
        public bool IsLockForever { get; set; }

        public DateTime? ExpiredDate { get; set; }
    }
}

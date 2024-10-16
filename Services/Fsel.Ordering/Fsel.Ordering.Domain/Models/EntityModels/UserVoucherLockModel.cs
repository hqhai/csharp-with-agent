// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class UserVoucherLockModel : BaseModel
    {
        public int Count { get; set; }

        public bool IsLockForever { get; set; }

        public DateTime? ExpiredDate { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class ApprovalTimeConfig : Entity
    {
        public EnumApprovalTime ApprovalTimeType { get; set; }

        public long ExpiredTime { get; set; }
    }
}

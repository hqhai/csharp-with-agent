// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class ApprovalTimeConfig : Entity
    {
        public EnumApprovalTime ApprovalType { get; set; }

        public long ExpiredTime { get; set; }

        public ICollection<ApprovalLog> ApprovalLogs { get; set; } = new List<ApprovalLog>();
    }
}

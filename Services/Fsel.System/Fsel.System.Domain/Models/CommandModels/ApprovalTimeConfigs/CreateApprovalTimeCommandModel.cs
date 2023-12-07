// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ApprovalTimeConfigs
{
    using Fsel.Shared.Enums;
    public class CreateApprovalTimeCommandModel
    {
        public IList<ApprovalTimeConfigCommandModel>? ApprovalTimeConfigs { get; set; }
    }

    public class ApprovalTimeConfigCommandModel
    {
        public EnumApprovalTime ApprovalTimeType { get; set; }

        public long ExpiredTime { get; set; }
    }
}

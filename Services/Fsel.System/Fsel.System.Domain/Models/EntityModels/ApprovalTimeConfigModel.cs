// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ApprovalTimeConfigModel : BaseModel
    {
        public EnumApprovalTime ApprovalType { get; set; }

        public long ExpiredTime { get; set; }
    }
}

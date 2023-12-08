// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ApprovalLog
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CreateApprovalLogCommandModel
    {
        public IList<ApprovalLogCommandModel>? ApprovalLogCommandModels { get; set; }
    }

    public class ApprovalLogCommandModel : BaseModel
    {
        public Guid ObjectId { get; set; }

        public DateTime ExpiredDate { get; set; }

        public EnumApprovalLogStatus Status { get; set; }

        public IList<Guid>? UserIds { get; set; }

        public Guid ApprovalTimeConfigId { get; set; }
    }

}

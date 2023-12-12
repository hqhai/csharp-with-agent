// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ApprovalLog
{
    using Fsel.Shared.Enums;

    public class CreateApprovalLogCommandModel
    {
        public Guid? ObjectId { get; set; }

        public DateTime ExpiredDate { get; set; }

        public EnumApprovalTime ApprovalType { get; set; }
    }

}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ApprovalLog
{
    public class CreateApprovalLogCommandModel
    {
        public IList<ApprovalLogCommandModel>? ApprovalLogCommandModels { get; set; }
    }

    public class ApprovalLogCommandModel
    {
        public Guid? Id { get; set; }
        public Guid? ObjectId { get; set; }

        public DateTime ExpiredDate { get; set; }

        public Guid ApprovalTimeConfigId { get; set; }
    }

}

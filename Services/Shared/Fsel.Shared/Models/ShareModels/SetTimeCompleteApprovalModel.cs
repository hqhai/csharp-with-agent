// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class SetTimeCompleteApprovalModel
    {
        public DateTime StartDate { get; set; }

        public Guid ObjectId { get; set; }

        public EnumApprovalTime ApprovalType { get; set; }

        public IList<Guid>? UserIds { get; set; }
    }
}

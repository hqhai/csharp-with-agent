// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class SetTimeCompleteApprovalModel
    {
        public DateTime ExpiredDate { get; set; }

        public Guid ObjectId { get; set; }

        public EnumApprovalTime ApprovalType { get; set; }
    }
}

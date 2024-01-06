// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class ApprovalLog : Entity
    {
        public DateTime ExpiredDate { get; set; }

        public EnumApprovalLogStatus Status { get; set; }

        public Guid ObjectId { get; set; }

        public string? UserIdsStr { get; set; }

        [NotMapped]
        public IList<Guid>? UserIds
        {
            get
            {
                return ConvertHelper.Deserialize<IList<Guid>>(UserIdsStr);
            }
            set { UserIdsStr = ConvertHelper.Serialize(value); }
        }

        public Guid ApprovalTimeConfigId { get; set; }

        public ApprovalTimeConfig? ApprovalTimeConfig { get; set; }

    }
}

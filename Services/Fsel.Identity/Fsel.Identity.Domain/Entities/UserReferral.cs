// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;

    public class UserReferral : Entity
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }

        public EnumUserReferralType Type { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FeatureMissionStr { get; set; }

        [NotMapped]
        public IList<UserReferralToken>? FeatureMissions
        {
            get { return ConvertHelper.Deserialize<IList<UserReferralToken>>(FeatureMissionStr); }
            set { FeatureMissionStr = ConvertHelper.Serialize(value); }
        }

        public User? Sender { get; set; }
        public User? Receiver { get; set; }
    }
}

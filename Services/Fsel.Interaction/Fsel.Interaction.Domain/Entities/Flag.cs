// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class Flag : Entity
    {
        public EnumFlagIssue FlagIssue { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FeedBack { get; set; }

        public EnumFlagStatus Status { get; set; }

        public EnumInteractionType Type { get; set; }
        public Guid? ObjectId { get; set; }
    }
}

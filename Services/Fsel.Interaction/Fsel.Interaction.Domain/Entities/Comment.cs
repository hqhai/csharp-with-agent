// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class Comment : Entity
    {
        /// <summary>
        /// Content
        /// </summary>
        [MaxLength(2000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Content { get; set; }

        public EnumCommentStatus Status { get; set; }
        public int LikeNumber { get; set; }
        public Guid ObjectId { get; set; }

        public Guid UserId { get; set; }
    }
}

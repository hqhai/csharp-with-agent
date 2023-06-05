// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class Comments : Entity
    {
        public string? Content { get; set; }
        public EnumCommentStatus Status { get; set; }
        public int LikeNumber { get; set; }
        public Guid ObjectId { get; set; }

        public Guid UserId { get; set; }
    }
}

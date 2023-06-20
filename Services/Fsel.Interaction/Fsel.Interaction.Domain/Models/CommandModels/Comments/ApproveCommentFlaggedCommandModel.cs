// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.Comments
{
    using System;

    public class ApproveCommentFlaggedCommandModel
    {
        public Guid CommentId { get; set; }

        public bool IsApprove { get; set; }
    }
}

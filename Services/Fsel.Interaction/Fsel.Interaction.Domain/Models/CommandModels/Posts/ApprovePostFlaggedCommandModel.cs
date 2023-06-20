// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.Posts
{
    using System;

    public class ApprovePostFlaggedCommandModel
    {
        public Guid PostId { get; set; }

        public bool IsApprove { get; set; }
    }
}

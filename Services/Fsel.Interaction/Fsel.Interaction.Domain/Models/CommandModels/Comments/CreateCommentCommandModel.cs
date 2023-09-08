// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.Comments
{
    using System;

    public class CreateCommentCommandModel
    {
        public Guid ObjectId { get; set; }

        public string? Content { get; set; }
    }
}

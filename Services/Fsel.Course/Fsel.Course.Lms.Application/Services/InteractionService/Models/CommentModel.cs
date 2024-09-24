// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.InteractionService.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CommentModel : BaseModel
    {
        public string? Content { get; set; }

        public Guid ObjectId { get; set; }

        public Guid UserId { get; set; }

        public EnumInteractionType Type { get; set; }
    }
}

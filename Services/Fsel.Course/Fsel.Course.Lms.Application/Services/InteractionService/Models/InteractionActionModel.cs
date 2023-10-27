// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.InteractionService.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class InteractionActionModel : BaseModel
    {
        public Guid ObjectId { get; set; }

        public Guid UserId { get; set; }

        public int LikeNumber { get; set; }

        public int CommentNumber { get; set; }

        public bool IsDisable { get; set; }

        public bool IsLiked { get; set; }
    }
}

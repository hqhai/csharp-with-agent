// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;

    public class CommentListModel : BaseModel
    {
        public bool IsLiked { get; set; }

        public int LikeNumber { get; set; }

        public string? Comment { get; set; }
    }
}

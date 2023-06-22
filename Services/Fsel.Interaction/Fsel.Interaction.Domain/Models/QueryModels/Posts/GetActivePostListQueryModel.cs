// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.QueryModels.Posts
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class GetActivePostListQueryModel : BaseQueryModel
    {
        public EnumPostStatus? Status { get; set; }

        public EnumPostType PostType { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.QueryModels.Posts
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class GetActivePostListQueryModel : BaseQueryModel
    {
        public EnumPostType PostType { get; set; }

        public string? TopicTagName { get; set; }

        public override string? SortField { get; set; }
    }
}

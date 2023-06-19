// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.QueryModels.Posts
{
    using Fsel.Shared.Enums;

    public class GetActivePostListQueryModel
    {
        public EnumPostStatus? Status { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.QueryModels.Posts
{
    using Fsel.Shared.Enums;

    public class GetPostsByStudentQueryModel
    {
        public EnumPostStatus? Status { get; set; }
    }
}

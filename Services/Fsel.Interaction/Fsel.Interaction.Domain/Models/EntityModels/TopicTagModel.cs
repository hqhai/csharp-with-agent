// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Domain.Entities;

    public class TopicTagModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Color { get; set; }

        public IList<PostTag>? PostTags { get; set; }
    }
}

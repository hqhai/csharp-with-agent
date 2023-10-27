// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class TopicTagModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Color { get; set; }
        public int? PostCount { get; set; }
    }
}

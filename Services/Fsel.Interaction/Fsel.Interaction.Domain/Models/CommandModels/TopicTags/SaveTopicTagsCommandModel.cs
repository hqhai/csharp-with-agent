// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.TopicTags
{
    using System.Collections.Generic;

    public class SaveTopicTagsCommandModel
    {
        public IList<SaveTopicTagCommandModel>? TopicTags { get; set; }
    }
}

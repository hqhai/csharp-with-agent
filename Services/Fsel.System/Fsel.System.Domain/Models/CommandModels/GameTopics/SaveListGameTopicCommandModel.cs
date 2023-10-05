// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.GameTopics
{
    public class SaveListGameTopicCommandModel
    {
        public IList<SaveGameTopicCommandModel>? GameTopics { get; set; }
    }
}

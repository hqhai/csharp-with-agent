// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class QuestBoardQueueModel
    {
        public Guid StudentId { get; set; }

        public IList<EnumQuestBoardCategory>? Categories { get; set; }

        public float AchievedPoint { get; set; }

        public Guid ObjectId { get; set; }
    }
}

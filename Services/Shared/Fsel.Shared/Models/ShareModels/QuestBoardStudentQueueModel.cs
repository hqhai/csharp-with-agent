// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class QuestBoardStudentQueueModel
    {
        public Guid ObjectId { get; set; }
        public Guid StudentId { get; set; }
        public EnumQuestBoardType QuestBoardType { get; set; }
        public EnumQuestBoardCategory QuestBoardCategory { get; set; }
    }
}

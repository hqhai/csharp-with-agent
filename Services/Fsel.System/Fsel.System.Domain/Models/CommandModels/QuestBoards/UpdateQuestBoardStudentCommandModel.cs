// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.QuestBoards
{
    using Fsel.Shared.Enums;

    public class UpdateQuestBoardStudentCommandModel
    {
        public Guid StudentId { get; set; }
        public EnumQuestBoardType QuestBoardType { get; set; }
        public EnumQuestBoardCategory QuestBoardCategory { get; set; }
    }
}

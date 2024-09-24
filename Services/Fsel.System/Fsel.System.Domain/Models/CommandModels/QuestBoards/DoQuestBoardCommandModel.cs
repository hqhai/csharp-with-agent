// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.QuestBoards
{
    using Fsel.Shared.Enums;

    public class DoQuestBoardCommandModel
    {
        public Guid StudentID { get; set; }
        public EnumQuestBoardType Type { get; set; }
        public EnumQuestBoardCategory Category { get; set; }
        public int Value { get; set; }
    }
}

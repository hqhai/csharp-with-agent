// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.QuestBoards
{
    public class ReceiveTokenFromQuestBoardDoneCommandModel
    {
        public bool IsQuestBoardOverall { get; set; }
        public Guid? QuestBoardId { get; set; }
        public Guid? QuestBoardOverallId { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.QuestBoards
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class QuestBoardStudent : Entity
    {
        public Guid QuestBoardId { get; set; }
        public Guid StudentId { get; set; }
        public Guid? QuestBoardOverallStudentId { get; set; }
        public int CurrentValue { get; set; }
        public int Token { get; set; }
        public int? Energy { get; set; }
        public EnumQuestBoardStudentStatus Status { get; set; }
        public QuestBoard? QuestBoard { get; set; }
        public QuestBoardOverallStudent? QuestBoardOverallStudent { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.QuestBoards
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System;

    public class QuestBoardOverallStudent : Entity
    {
        public Guid QuestBoardOverallId { get; set; }
        public Guid StudentId { get; set; }
        public int CurrentValue { get; set; }
        public int Token { get; set; }
        public EnumQuestBoardOverallStudentStatus Status { get; set; }
        public QuestBoardOverall? QuestBoardOverall { get; set; }
        public ICollection<QuestBoardStudent> QuestBoardStudents { get; set; } = new List<QuestBoardStudent>();
    }
}

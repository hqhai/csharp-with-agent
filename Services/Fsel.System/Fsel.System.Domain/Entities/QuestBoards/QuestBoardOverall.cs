// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.QuestBoards
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class QuestBoardOverall : Entity
    {
        public EnumQuestBoardType Type { get; set; }
        public int TargetValue { get; set; }
        public int Token { get; set; }
        public ICollection<QuestBoardOverallStudent> QuestBoardOverallStudents { get; set; } = new List<QuestBoardOverallStudent>();
    }
}

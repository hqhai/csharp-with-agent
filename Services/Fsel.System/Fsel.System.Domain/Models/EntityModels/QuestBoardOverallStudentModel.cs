// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class QuestBoardOverallStudentModel : BaseModel
    {
        public Guid QuestBoardOverallId { get; set; }
        public Guid StudentId { get; set; }
        public int CurrentValue { get; set; }
        public int Token { get; set; }
        public EnumQuestBoardOverallStudentStatus Status { get; set; }
    }
}

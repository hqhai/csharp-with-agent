// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using global::System;

    public class QuestBoardStudentModel : BaseModel
    {
        public Guid QuestBoardId { get; set; }
        public Guid StudentId { get; set; }
        public Guid? QuestBoardOverallStudentId { get; set; }
        public int CurrentValue { get; set; }
        public int Token { get; set; }
        public int? Energy { get; set; }
        public EnumQuestBoardStudentStatus Status { get; set; }
    }
}

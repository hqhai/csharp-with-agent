// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class QuestBoardModel : BaseModel
    {
        public EnumQuestBoardType Type { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? ImagePath { get; set; }

        public EnumQuestBoardCategory Category { get; set; }

        public int TargetValue { get; set; }

        public int CurrentValue { get; set; }

        public bool IsFinish { get; set; }

        public int Token { get; set; }

        public int? Energy { get; set; }

        public EnumRepeatType? RepeatType { get; set; }

        public bool IsActive { get; set; }

        public EnumQuestBoardStudentStatus? Status { get; set; }
    }
}

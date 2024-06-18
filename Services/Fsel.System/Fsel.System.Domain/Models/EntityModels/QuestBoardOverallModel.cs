// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class QuestBoardOverallModel : BaseModel
    {
        public EnumQuestBoardType Type { get; set; }
        public int TargetValue { get; set; }
        public int Token { get; set; }
        public EnumQuestBoardOverallStudentStatus? Status { get; set; }
    }
}

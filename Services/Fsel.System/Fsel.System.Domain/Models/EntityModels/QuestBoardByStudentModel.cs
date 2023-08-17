// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class QuestBoardByStudentModel : BaseModel
    {
        public string? Name { get; set; }
        public long NumberOfStars { get; set; }
        public double Percent { get; set; }
        public EnumQuestBoardCategory Category { get; set; }
        public EnumQuestBoardStatus Status { get; set; }
    }
}

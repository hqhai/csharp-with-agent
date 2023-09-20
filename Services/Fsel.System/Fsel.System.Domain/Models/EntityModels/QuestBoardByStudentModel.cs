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
        public Guid? ObjectId { get; set; }
        public IList<Guid>? PackageIds { get; set; }
        public EnumQuestBoardCategory Category { get; set; }
        public EnumQuestBoardStudentStatus? Status { get; set; }
        public EnumRepeatType? RepeatType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

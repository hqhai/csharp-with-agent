// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.SystemService.Models
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class QuestBoardModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public EnumQuestBoardType Type { get; set; }
        public EnumQuestBoardCategory Category { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumberOfStars { get; set; }
        public bool IsLifeTime { get; set; }
        public EnumRepeatType? RepeatType { get; set; }
        public IList<Guid>? PackageIds { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public Guid? DependentId { get; set; }
    }
}

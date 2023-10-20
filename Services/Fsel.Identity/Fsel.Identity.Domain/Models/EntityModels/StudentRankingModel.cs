// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentRankingModel : BaseModel
    {
        public Guid StudentId { get; set; }

        public Guid? UserId { get; set; }

        public int DailyStreak { get; set; }

        public double TotalScore { get; set; }

        public int PositionChange { get; set; }

        public int CurrentPosition { get; set; }

        public string? AvatarPath { get; set; }

        public string? FullName { get; set; }

        public EnumCourseLevel Level { get; set; }

    }
}

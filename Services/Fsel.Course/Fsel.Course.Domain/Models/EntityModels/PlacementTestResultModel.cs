// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class PlacementTestResultModel : BaseModel
    {
        public double Percent { get; set; }
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
        public EnumPlacementTestLevel Level { get; set; }
        public EnumResultStatus Status { get; set; }
        public Guid StudentId { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Collections.Generic;

    public class UnitScoreModel
    {
        public IList<UnitSkillScoreModel>? UnitSkillScores { get; set; }
        public double Percent { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.SkillScoresConfigs
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Helpers;

    public class TestSkillScores : SkillScores
    {
        private double _percentProgress;

        public EnumResultStatus? Status { get; set; }

        public double PercentProgress
        {
            get
            {
                return TotalQuestion > 0 ? NumberHelper.GetPercent(CountQuestion, TotalQuestion) : _percentProgress;
            }
            set { _percentProgress = TotalQuestion > 0 ? NumberHelper.GetPercent(CountQuestion, TotalQuestion) : value; }
        }
    }
}

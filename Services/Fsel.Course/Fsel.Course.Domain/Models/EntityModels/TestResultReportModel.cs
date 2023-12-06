// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.IEntities;

    public class TestResultReportModel : BaseResultScoreModel, IHighestStreak
    {
        public double? Score { get; set; }

        public double? WorkingTime { get; set; }

        public int? HighestStreak { get; set; }
    }
}

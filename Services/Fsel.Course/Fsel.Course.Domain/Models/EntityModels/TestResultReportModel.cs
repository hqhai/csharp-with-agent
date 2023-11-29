// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class TestResultReportModel : BaseResultScoreModel
    {
        public double? Score { get; set; }

        public double? WorkingTime { get; set; }

        public int? HighestStreak { get; set; }
    }
}

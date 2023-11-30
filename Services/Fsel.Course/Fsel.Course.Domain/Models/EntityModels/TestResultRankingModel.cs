// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class TestResultRankingModel : BaseResultScoreModel
    {
        public bool IsCurrentStudent { get; set; }

        public double? WorkingTime { get; set; }

        public string? AvatarPath { get; set; }

        public string? FullName { get; set; }

        public double? Score { get; set; }
    }
}

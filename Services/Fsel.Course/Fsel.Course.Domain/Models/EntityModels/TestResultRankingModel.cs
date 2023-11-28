// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class TestResultRankingModel : BaseResultScoreModel
    {
        public bool IsCurrentStudent { get; set; }

        public double TimeSpend { get; set; }

        public string? AvatarPath { get; set; }

        public string? FullName { get; set; }
    }
}

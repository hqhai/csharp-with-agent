// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class FinalTestResultRankingModel : BaseResultScoreModel
    {
        public bool IsCurrentStudent { get; set; }

        public double TimeSpend { get; set; }
    }
}

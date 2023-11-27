// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;

    public class FinalTestResultRankingModel : BaseResultScoreModel
    {
        public Guid FinalTestId { get; set; }
        public Guid CourseId { get; set; }

        public bool IsCurrentStudent { get; set; }

        public double TimeSpend { get; set; }
    }
}

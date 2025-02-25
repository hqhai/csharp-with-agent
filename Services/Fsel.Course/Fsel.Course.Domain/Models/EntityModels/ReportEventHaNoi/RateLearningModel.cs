// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class RateLearningModel
    {
        public decimal? PercentJoinA1 { get; set; }
        public decimal? PercentJoinA2 { get; set; }
        public decimal? PercentJoinB1 { get; set; }
        public decimal? PercentJoinB1Plus { get; set; }
        public decimal? PercentJoinB2 { get; set; }
        public decimal? PercentJoinC1 { get; set; }
        public decimal? PercentJoinMS1 { get; set; }
        public decimal? PercentJoinMS2 { get; set; }
        public decimal? PercentJoinMS3 { get; set; }
    }
}

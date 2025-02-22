// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class TotalDetailLearningQualityModel
    {
        public string? DistrictName { get; set; }
        public string? GroupName { get; set; }
        public string? SchoolName { get; set; }
        public int? LowRate { get; set; }
        public int? DecentRate { get; set; }
        public int? GoodRate { get; set; }
        public int? ExcellentRate { get; set; }
    }
}

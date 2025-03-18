// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class SummaryDataOnCityModel
    {
        public string? GroupName { get; set; }
        public string? DistrictName { get; set; }
        public string? SchoolName { get; set; }
        public int? ActiveCount { get; set; }
        public decimal? PercentageActive { get; set; }
        public decimal? PercentageChange { get; set; }

    }
}

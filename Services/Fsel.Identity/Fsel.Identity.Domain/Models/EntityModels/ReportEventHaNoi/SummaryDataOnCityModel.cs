// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class SummaryDataOnCityModel
    {
        public string? GroupedValue { get; set; }
        public int? ActiveCount { get; set; }
        public decimal? PercentageActive { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class TotalDetailEvaluateInputResultModel
    {
        public string? DistrictName { get; set; }

        public string? GroupName { get; set; }

        public string? SchoolName { get; set; }

        public int? StudentDonePT { get; set; }

        public int? StudentProcessPT { get; set; }

        public int? StudentNewPT { get; set; }
    }
}

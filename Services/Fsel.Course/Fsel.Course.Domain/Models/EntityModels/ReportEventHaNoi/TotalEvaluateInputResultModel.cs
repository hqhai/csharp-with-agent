// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class TotalEvaluateInputResultModel
    {
        public int? TotalStudent { get; set; }

        public decimal? PercentTotalStudent { get; set; }

        public int? StudentDonePT { get; set; }

        public decimal? PercentStudentDonePT { get; set; }

        public int? StudentProcessPT { get; set; }

        public decimal? PercentStudentProcessPT { get; set; }

        public int? StudentNewPT { get; set; }

        public decimal? PercentStudentNewPT { get; set; }
    }
}

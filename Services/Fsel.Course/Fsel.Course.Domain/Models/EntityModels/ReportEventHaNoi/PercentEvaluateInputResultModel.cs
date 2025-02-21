// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class PercentEvaluateInputResultModel
    {
        public decimal PercentStudentDonePT { get; set; }

        public decimal PercentStudentProcessPT { get; set; }

        public decimal PercentStudentNewPT { get; set; }
    }
}

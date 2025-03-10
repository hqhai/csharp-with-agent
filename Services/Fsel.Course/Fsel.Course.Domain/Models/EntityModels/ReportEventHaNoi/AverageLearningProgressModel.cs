// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class AverageLearningProgressModel
    {
        public string? GroupedValue { get; set; }

        public decimal? PercentLessonDone { get; set; }
    }
}

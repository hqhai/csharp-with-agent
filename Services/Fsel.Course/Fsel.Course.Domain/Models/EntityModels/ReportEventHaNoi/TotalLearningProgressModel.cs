// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class TotalLearningProgressModel
    {
        public int? TotalStudent { get; set; }

        public int? TotalStudentJoin { get; set; }

        public int? LessonDone { get; set; }

        public decimal? PercentLessonDone { get; set; }
    }
}

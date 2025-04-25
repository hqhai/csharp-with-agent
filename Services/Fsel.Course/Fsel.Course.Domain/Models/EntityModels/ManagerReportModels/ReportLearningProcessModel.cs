// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class ReportLearningProcessModel
    {
        public Guid StudentId { get; set; }
        public string? SchoolClass { get; set; }
        public bool IsMaxHoursCompleted { get; set; }
        public int DisplayOrder { get; set; }
        public int TotalLessonDone { get; set; }
        public int TotalUnitDone { get; set; }
    }
}

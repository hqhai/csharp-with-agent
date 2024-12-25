// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    public class OverallReportLearningProgressModel
    {
        public long TotalStudent { get; set; }
        public IList<CourseLevelProgressModel>? CourseLevelProgresses { get; set; }
        public string? ContentAverageProgress { get; set; }
    }
}

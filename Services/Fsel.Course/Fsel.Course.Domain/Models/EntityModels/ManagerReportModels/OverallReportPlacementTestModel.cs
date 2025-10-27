// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    public class OverallReportPlacementTestModel
    {
        public long TotalStudent { get; set; }
        public long TotalPlacementTest { get; set; }
        public long TotalCompletePlacementTest { get; set; }
        public IList<CourseLevelProgressModel> CourseLevelProgresses { get; set; } = new List<CourseLevelProgressModel>();
    }
}

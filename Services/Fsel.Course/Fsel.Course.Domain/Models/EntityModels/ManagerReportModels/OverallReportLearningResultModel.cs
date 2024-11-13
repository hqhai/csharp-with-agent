// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    public class OverallReportLearningResultModel
    {
        public long TotalStudent { get; set; }
        public double OverallAvgPercent { get; set; }
        public IList<CourseLevelProgressModel>? CourseLevelProgresses { get; set; }
        public IList<OverallModuleReportModel>? OverallModules { get; set; }
    }

    public class OverallModuleReportModel
    {
        public int Index { get; set; }
        public int DisplayOrder { get; set; }
        public string? Type { get; set; }
        public double? Percent { get; set; }
        public double? Score { get; set; }
        public long? TotalStudent { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    public class CourseLevelProgressModel
    {
        public Guid LevelId { get; set; }
        public string? LevelName { get; set; }
        public int TotalStudent { get; set; }
        public IList<OverallTestResultModel>? OverallTestResults { get; set; }
    }

    public class OverallTestResultModel
    {
        public string? Type { get; set; }
        public int Index { get; set; }
        public double Score { get; set; }
        public int TotalStudent { get; set; }
    }
}

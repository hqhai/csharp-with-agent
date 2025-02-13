// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.BaseChartModels
{
    public class DashBoardDiligenceModel
    {
        public string? SchoolName { get; set; }

        public NumberStudentAccessModel NumberStudentAccessModel { get; set; } = new NumberStudentAccessModel();

        public BaseChartResultModel NumberStudentNotAccessModel { get; set; } = new BaseChartResultModel();

        public LearningResultReportModel LearningResultReportModel { get; set; } = new LearningResultReportModel();
    }

    public class NumberStudentAccessModel : BaseChartResultModel
    {
        public int Percent { get; set; }
    }

    public class LearningResultReportModel : StackBarChartsModel
    {
        public int Percent { get; set; }

        public long TotalTime { get; set; }

        public double AveragePerDay { get; set; }
    }
}

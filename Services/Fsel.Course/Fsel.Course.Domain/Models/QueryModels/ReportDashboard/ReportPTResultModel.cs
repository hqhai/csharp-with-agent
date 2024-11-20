// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ReportDashboard
{
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels.BaseChart;
    using Fsel.Shared.Enums;

    public class ReportPTResultModel : BaseModel
    {
        public string? SchoolName { get; set; }
        public OverallStaticModel OverallStatic { get; set; } = new OverallStaticModel();
        public BaseChartResultModel OverallEvaluation {  get; set; } = new BaseChartResultModel();
        public BaseChartResultModel AmountStudentByLevel { get; set; } = new BaseChartResultModel();
        public StackBarCharts AmountStudentByLevelAndClass { get; set; } = new StackBarCharts();
    }
    public class OverallStaticModel
    {
        public int TotalStudentInSchool { get; set; }
        public int NumberStudentFinishPT { get; set; }
        public int NumberStudentNotFinishPT { get; set; }

    }
}

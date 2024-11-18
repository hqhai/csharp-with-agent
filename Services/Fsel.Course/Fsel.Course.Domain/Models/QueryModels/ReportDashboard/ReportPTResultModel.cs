// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ReportDashboard
{
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Shared.Enums;

    public class ReportPTResultModel : BaseModel
    {
        public OverallStaticModel OverallStatic { get; set; } = new OverallStaticModel();
        public BaseChartResult OverallEvaluation {  get; set; } = new BaseChartResult();
        public BaseChartResult AmountStudentByLevel { get; set; } = new BaseChartResult();
        public StackBarCharts AmountStudentByLevelAndClass { get; set; } = new StackBarCharts();
    }

    public class StackBarCharts : BaseChartResult
    {
        public new IList<StackBarChart> DataCharts { get; set; } = new List<StackBarChart>();

    }
    public class OverallStaticModel
    {
        public int TotalStudentInSchool { get; set; }
        public int NumberStudentFinishPT { get; set; }
        public int NumberStudentNotFinishPT { get; set; }

    }
    public class StackBarChart
    {
        public string? Labels { get; set; }
        public IList<DataChart>? DataColumns { get; set; }

    }

}

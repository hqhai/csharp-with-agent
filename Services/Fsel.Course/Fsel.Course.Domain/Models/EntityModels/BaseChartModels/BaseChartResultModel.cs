// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.BaseChart
{
    using Fsel.Shared.Enums;

    public class BaseChartResultModel
    {
        public EnumChartType Type { get; set; }
        public IList<DataChart>? DataCharts { get; set; }
    }

    public class DataChart
    {
        public string? Label { get; set; }
        public int Value { get; set; }
    }

    public class StackBarCharts : BaseChartResultModel
    {
        public new IList<StackBarChart> DataCharts { get; set; } = new List<StackBarChart>();

    }

    public class StackBarChart
    {
        public string? Label { get; set; }
        public IList<DataChart>? DataColumns { get; set; }

    }
}

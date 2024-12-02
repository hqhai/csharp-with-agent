// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.BaseChartModels
{
    using Fsel.Shared.Enums;

    public class BaseChartResultModel
    {
        public EnumChartType Type { get; set; }
        public IList<DataChartModel>? DataCharts { get; set; }
    }

    public class DataChartModel
    {
        public string? Label { get; set; }
        public int Value { get; set; }
    }

    public class DataPieChartModel : DataChartModel
    {
        public int Percent { get; set; }
    }

    public class StackBarChartsModel : BaseChartResultModel
    {
        public new IList<StackBarChartModel> DataCharts { get; set; } = new List<StackBarChartModel>();
    }

    public class StackBarChartModel
    {
        public string? Label { get; set; }
        public IList<DataChartModel>? DataColumns { get; set; }
    }
}

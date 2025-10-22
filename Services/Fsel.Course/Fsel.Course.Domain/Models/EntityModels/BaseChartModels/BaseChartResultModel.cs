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
        public WeekSummary? Summary { get; set; }
        public new IList<StackBarChartModel> DataCharts { get; set; } = new List<StackBarChartModel>();
    }

    public class StackBarChartModel
    {
        public string? Label { get; set; }
        public string? Value { get; set; }
        public int NumericValue { get; set; }
        public DateTime? WeekStart { get; set; }
        public DateTime? WeekEnd { get; set; }
        public int Percent { get; set; }
        public IList<DataChartModel>? DataColumns { get; set; }
    }

    public class WeekSummary
    {
        public int CurrentOnTrack { get; set; }
        public int PrevOnTrack { get; set; }
        public int ChangeAbs { get; set; }
        public int ChangePercent { get; set; }      // -18 => -18%
    }
}

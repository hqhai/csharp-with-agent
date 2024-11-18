// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Shared.Enums;

    public class BaseChartResult
    {
        public EnumChartType Type { get; set; }
        public IList<DataChart> DataCharts { get; set; }
    }

    public class DataChart
    {
        public string? Label { get; set; }
        public int Value { get; set; }
    }
}

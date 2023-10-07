// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class FeatureAcessTimeChartModel
    {
        public EnumFeatureBussinessType? FeatureBussinessType { get; set; }

        public string? Label { get; set; }

        public IList<double>? ChartData { get; set; }
    }
}

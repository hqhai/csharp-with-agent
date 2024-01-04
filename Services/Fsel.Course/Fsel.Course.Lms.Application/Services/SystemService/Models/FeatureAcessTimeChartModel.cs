// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using Fsel.Shared.Enums;

    public class FeatureAcessTimeChartModel
    {
        public EnumFeatureBussinessType? FeatureBussinessType { get; set; }

        public string? Label { get; set; }

        public IList<FeatureAccessTimeByTypeModel>? FeatureAccessTimes { get; set; }
    }
}

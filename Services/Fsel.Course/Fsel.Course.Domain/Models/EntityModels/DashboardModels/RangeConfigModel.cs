// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.DashboardModels
{
    using System.Collections.Generic;

    public class RotationConfig
    {
        public int PeriodMinutes { get; set; } = 60; // mặc định 60 phút
        public IList<RangeConfigModel> Rules { get; set; } = new List<RangeConfigModel>();
    }

    public class RangeConfigModel
    {
        public RuleMatchMode MatchMode { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public IList<string> Messages { get; set; } = new List<string>();
    }

    public enum RuleMatchMode
    {
        All,
        Any
    }
}

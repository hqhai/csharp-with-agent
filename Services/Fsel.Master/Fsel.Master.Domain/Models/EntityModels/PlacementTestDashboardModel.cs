// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;

    public class PlacementTestDashboardModel
    {
        public PlacementTestSummaryModel? Summary { get; set; }

        public IList<LevelDistributionModel>? LevelDistributions { get; set; }
    }

    public class PlacementTestSummaryModel
    {
        public int TotalRegistered { get; set; }

        public int TotalStarted { get; set; }

        public int TotalCompleted { get; set; }
    }

    public class LevelDistributionModel
    {
        public Guid LevelId { get; set; }

        public string? LevelName { get; set; }

        public Guid ProgramId { get; set; }

        public string? ProgramName { get; set; }

        public int StudentCount { get; set; }

        public int? DisplayOrder { get; set; }

        public double Percentage { get; set; }
    }
}

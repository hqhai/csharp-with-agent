// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class PlacementTestReportOveallModel
    {
        public string? FullName { get; set; }
        public EnumCourseLevel? SuggetLevel { get; set; }
        public string? CurrentLevel { get; set; }
        public bool IsPreA1 { get; set; }
        public PlacementTestReportConfigModel? PlacementTestReportConfig { get; set; }
        public PlacementTestReportViewConfigModel? PlacementTestViewReport { get; set; }
    }
}

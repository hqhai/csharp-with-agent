// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class PlacementTestReportOveallModel
    {
        public string? FullName { get; set; }
        public EnumCourseLevel? SuggetLevel { get; set; }
        public EnumCourseLevel? CurrentLevel { get; set; }
        public PlacementTestReportConfigModel? PlacementTestReportConfig { get; set; }
        public PlacementTestReportViewConfigModel? PlacementTestViewReport { get; set; }
    }
}

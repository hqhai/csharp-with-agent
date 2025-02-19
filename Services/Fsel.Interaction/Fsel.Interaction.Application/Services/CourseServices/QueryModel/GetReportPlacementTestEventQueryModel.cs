// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.CourseServices.QueryModel
{
    using Fsel.Shared.Enums;

    public class GetReportPlacementTestEventQueryModel
    {
        public string? EventCodeStr { get; set; }
        public EnumEducationLevel EducationLevel { get; set; }
        public string? DistrictName { get; set; }
    }
}

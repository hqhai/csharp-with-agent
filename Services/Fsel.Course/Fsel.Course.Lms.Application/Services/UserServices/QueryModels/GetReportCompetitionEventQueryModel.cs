// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.QueryModels
{
    using Fsel.Shared.Enums;

    public class GetReportCompetitionEventQueryModel
    {
        public string? EventCodeStr { get; set; }
        public string? DistrictName { get; set; }
        public EnumEducationLevel EducationLevel { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.QueryModels
{
    using Fsel.Shared.Enums;

    public class GetReportCompetitionEventQueryModel
    {
        public string? EventCodeStr { get; set; }
        public string? DistrictName { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumEducationLevel? EducationLevel { get; set; }
        public Guid? StudentId { get; set; }
        public string? UserNameStr { get; set; }
    }
}

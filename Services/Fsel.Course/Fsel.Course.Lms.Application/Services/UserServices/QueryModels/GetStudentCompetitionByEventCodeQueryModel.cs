// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class GetStudentCompetitionByEventCodeQueryModel : BaseQueryModel
    {
        public EnumCourseType CourseType { get; set; }
        public string? EventCode { get; set; }
        public int WeekNumber { get; set; }
    }
}

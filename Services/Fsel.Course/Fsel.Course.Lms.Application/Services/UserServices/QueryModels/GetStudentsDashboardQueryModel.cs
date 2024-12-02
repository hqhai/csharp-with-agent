// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.QueryModels
{
    using Fsel.Shared.Enums;

    public class GetStudentsDashboardQueryModel
    {
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
        public IList<string>? SchoolClasses { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.Model
{
    using Fsel.Shared.Enums;

    public class GetTokenQueryModel
    {
        public EnumTokenFeature Feature { get; set; }
        public EnumTokenMission Mission { get; set; }
        public EnumCourseType? CourseType { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService.CommandModels
{
    using Fsel.Shared.Enums;

    public class SavePlacementTestDoneCommandModel
    {
        public Guid StudentId { get; set; }
        public bool? IsSendLevel { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}

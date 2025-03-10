// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.CourseServices.Models
{
    using Fsel.Shared.Enums;

    public class StudentSettingModel
    {
        public Guid StudentId { get; set; }

        public EnumCourseLevel? Level { get; set; }

        public EnumTrialRegistrationStatus? Status { get; set; }
    }
}

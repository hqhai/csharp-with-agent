// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TrainingServices.Models
{
    using Fsel.Shared.Enums;

    public class GetClassListStatusNewModel
    {
        public IList<Guid>? CourseIds { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService.Model
{
    using Fsel.Shared.Enums;

    public class StudentCompetitionStatQueryModel
    {
        public IList<Guid>? StudentIds { get; set; }

        public EnumCourseType CourseType { get; set; }
    }
}

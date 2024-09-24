// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService.Model
{
    public class CourseIntegrationQueryModel
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public IList<Guid>? UserIds { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    public class FeatureAccessTimesByCourseIdsQueryModel
    {
        public IList<Guid>? CourseIds { get; set; }
        public Guid UserId { get; set; }
    }
}

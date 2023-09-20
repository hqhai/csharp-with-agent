// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    public class FeatureAccessTimesByLessonIdsQueryModel
    {
        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public IList<Guid>? LessonIds { get; set; }
        public Guid UserId { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    public class GetFeatureAccessTimesByLessonIdsQueryModel
    {
        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public IList<Guid>? LessonIds { get; set; }
        public Guid UserId { get; set; }
    }
}

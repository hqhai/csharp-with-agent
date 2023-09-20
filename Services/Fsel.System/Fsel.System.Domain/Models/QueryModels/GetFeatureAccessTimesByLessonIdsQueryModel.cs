// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    public class GetFeatureAccessTimesByLessonIdsQueryModel
    {
        public Guid CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public IList<Guid> LessonIds { get; set; } = new List<Guid>();
        public Guid UserId { get; set; }
    }
}

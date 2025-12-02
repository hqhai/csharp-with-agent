// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.Lessons.V1i2
{
    public class SearchLessonQueryModel
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid? UserId { get; set; }
    }
}

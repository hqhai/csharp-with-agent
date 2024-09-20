// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService.Model
{
    public class UnitResultModel
    {
        public Guid UserId { get; set; }

        public string? Name { get; set; }

        public string? CurrentLesson { get; set; }

        public int? LessonCompleted { get; set; }

        public DateTime? StartCourse { get; set; }

        public DateTime? EndCourse { get; set; }

        public string? CourseLevel { get; set; }

        public DateTime? DateEdit { get; set; }
    }
}

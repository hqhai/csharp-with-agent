// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.LmsCourseService.Model
{
    public class LessonResultModel
    {
        public Guid UserId { get; set; }

        public string? CurrentLesson { get; set; }

        public int? LessonCompleted { get; set; }
    }
}

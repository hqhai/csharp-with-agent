// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.CourseService.Model
{
    using Fsel.Shared.Enums;

    public class CourseResultModel
    {
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public string? Status { get; set; }
    }
}

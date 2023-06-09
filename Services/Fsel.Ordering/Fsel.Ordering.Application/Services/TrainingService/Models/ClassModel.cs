// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.TrainingService.Models
{
    using Fsel.Shared.Enums;

    public class ClassModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EnumClassType Status { get; set; }
        public Guid CourseId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
    }
}

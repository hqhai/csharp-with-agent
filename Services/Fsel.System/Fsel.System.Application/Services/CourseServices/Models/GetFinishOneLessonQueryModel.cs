// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.CourseServices.Models
{
    using Fsel.Shared.Enums;
    using global::System;

    public class GetFinishOneLessonQueryModel
    {
        public Guid StudentId { get; set; }
        public EnumRepeatType? RepeatType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

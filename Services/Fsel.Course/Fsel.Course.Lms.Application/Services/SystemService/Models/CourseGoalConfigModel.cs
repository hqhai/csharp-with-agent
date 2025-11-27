// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class CourseGoalConfigModel : BaseModel
    {
        public int LessonsPerWeek { get; set; }
        public Guid CourseId { get; set; }
        public Guid CourseGoalId { get; set; }
        public string? CourseName { get; set; }
    }
}

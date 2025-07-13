// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;

    public class OverallModuleLearnModel
    {
        public Guid CourseId { get; set; }
        public string? CourseName { get; set; }
        public int Count { get; set; }
    }
}

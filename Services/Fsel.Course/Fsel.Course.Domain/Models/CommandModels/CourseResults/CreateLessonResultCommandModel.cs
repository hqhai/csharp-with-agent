// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.CourseResults
{
    using System;
    using System.Collections.Generic;
    using Fsel.Course.Domain.Enums;

    public class CreateLessonResultCommandModel
    {
        public double Percent { get; set; }

        public EnumResultStatus Status { get; set; }
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid LessonId { get; set; }
        public Guid StudentId { get; set; }

        public ICollection<CreateVideoResultCommandModel>? VideoResults { get; set; }
    }
}

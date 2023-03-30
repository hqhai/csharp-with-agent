// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.CourseResults
{
    using Fsel.Course.Domain.Enums;

    public class CreateVideoResultCommandModel
    {
        public double Percent { get; set; }
        public int CorrectCount { get; set; }

        public int CorrectTotal { get; set; }
        public EnumResultStatus Status { get; set; }
        public Guid LessonResultId { get; set; }

        public Guid VideoId { get; set; }

        public Guid StudentId { get; set; }
    }
}

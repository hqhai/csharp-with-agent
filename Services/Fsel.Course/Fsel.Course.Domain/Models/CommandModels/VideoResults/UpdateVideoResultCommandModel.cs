// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.VideoResults
{
    using System;
    using Fsel.Course.Domain.Enums;

    public class UpdateVideoResultCommandModel
    {
        public Guid Id { get; set; }
        public double Percent { get; set; }
        public int CorrectCount { get; set; }
        public int CorrectTotal { get; set; }
        public EnumResultStatus Status { get; set; }
        public double NumberOfStars { get; set; }
        public string? Feedback { get; set; }
        public Guid LessonResultId { get; set; }
        public Guid VideoId { get; set; }
        public Guid StudentId { get; set; }
    }
}

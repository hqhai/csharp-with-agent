// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.VideoResults
{
    using System;

    public class ReviewLessonVideoCommandModel
    {
        public Guid VideoResultId { get; set; }
        public double NumberOfStars { get; set; }
        public string? Feedback { get; set; }
    }
}

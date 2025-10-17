// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;

    public class TrackingTimeModel
    {
        public string? EnumFeature { get; set; }
        public long? AccessTime { get; set; }
        public Guid? ObjectId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? LessonId { get; set; }

        public Guid UserId { get; set; }

        public string? UserAgent { get; set; }
    }

}

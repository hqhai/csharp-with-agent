// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.FeatureAccessTimes
{
    public class SaveFeatureAccessTimeCommandModel
    {
        public string? Type { get; set; }
        public long? AccessTime { get; set; }
        public Guid? CourseResultId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? LessonId { get; set; }
        public Guid? ObjectId { get; set; }
        public Guid UserId { get; set; }
        public string? UserAgent { get; set; }
    }
}

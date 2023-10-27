// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.FeatureAccessTimes
{
    using Fsel.Shared.Enums;

    public class SaveFeatureAccessTimeCommandModel
    {
        public EnumFeature EnumFeature { get; set; }
        public long? AccessTime { get; set; }
        public Guid? ObjectId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? LessonId { get; set; }
    }
}

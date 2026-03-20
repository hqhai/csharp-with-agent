// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using Fsel.Shared.Enums;

    public class FeatureAccessTimeQueryModel
    {
        public EnumFeature? EnumFeature { get; set; }
        public Guid UserId { get; set; }
        public Guid? CourseResultId { get; set; }
        public Guid? ObjectId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? LessonId { get; set; }
    }
}

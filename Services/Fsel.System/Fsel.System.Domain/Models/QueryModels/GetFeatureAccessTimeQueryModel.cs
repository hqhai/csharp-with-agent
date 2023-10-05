// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    using Fsel.Shared.Enums;

    public class GetFeatureAccessTimeQueryModel
    {
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }
        public Guid? LessonId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? ObjectId { get; set; }
        public EnumFeature? EnumFeature { get; set; }
    }
}

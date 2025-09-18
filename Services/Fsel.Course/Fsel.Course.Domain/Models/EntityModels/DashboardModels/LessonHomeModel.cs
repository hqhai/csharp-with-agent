// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.DashboardModels
{
    using Fsel.Shared.Enums;

    public class LessonHomeModel
    {
        public Guid? LessonId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? VideoId { get; set; }
        public Guid? ObjectId { get; set; }
        public EnumLessonOverviewStatus? Status { get; set; }
        public bool IsUnitFirst { get; set; }
        public string? Type { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class FeatureAccessTimeModel : BaseModel
    {
        public EnumFeature? EnumFeature { get; set; }
        public int Visit { get; set; }
        public long AccessTime { get; set; }
        public DateTime? LastVisited { get; set; }
        public DateTime? LearnLastVisited { get; set; }
        public Guid? ObjectId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? UnitId { get; set; }
        public Guid? LessonId { get; set; }
        public Guid? UserId { get; set; }
        public string? DeviceName { get; set; }
    }
}

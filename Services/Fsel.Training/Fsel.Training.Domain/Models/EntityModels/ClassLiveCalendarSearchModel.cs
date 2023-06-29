// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ClassLiveCalendarSearchModel : BaseModel
    {
        public string? Code { get; set; }
        public Guid CourseId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public DateTime LiveDate { get; set; }
        public Guid LiveTimeFrameId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? AccessLink { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Domain.Enums;

    public class ClassLiveCalendarSearchModel : BaseModel
    {
        public string? Code { get; set; }
        public Guid CourseId { get; set; }
        public bool IsActiveWorkFlow { get; set; }
        public bool IsActiveWorkPlan { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public DateTime LiveDate { get; set; }
        public Guid? LiveTimeFrameId { get; set; }
        public double? StartTime { get; set; }
        public double? EndTime { get; set; }
        public string? AccessLink { get; set; }
    }
}

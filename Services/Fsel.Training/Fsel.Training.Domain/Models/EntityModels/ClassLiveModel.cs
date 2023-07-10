// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ClassLiveModel : BaseModel
    {
        public string? Code { get; set; }
        public Guid CourseId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? LiveTimeFrameId { get; set; }
        public bool IsStatus { get; set; }
        public double? StartTime { get; set; }
        public double? EndTime { get; set; }
        public IList<DayOfWeek>? LiveDays { get; set; }
    }
}

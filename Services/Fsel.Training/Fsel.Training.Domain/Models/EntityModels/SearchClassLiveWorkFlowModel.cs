// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Domain.Enums;

    public class SearchClassLiveWorkFlowModel : BaseModel
    {
        public EnumWorkFlowType Type { get; set; }
        public string? Status { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid LiveTimeFrameId { get; set; }
        public IList<DayOfWeek>? LiveDays { get; set; }
        public string? ClassName { get; set; }
        public string? ClassCode { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public string? TeacherName { get; set; }
        public double? StartTime { get; set; }
        public double? EndTime { get; set; }
        public Guid? CourseId { get; set; }

        /* public ClassLiveCalendarModel? ClassLiveCalendar { get; set; }*/
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Training.Domain.Entities;
    using Fsel.Training.Domain.Enums;

    public class SearchClassLiveWorkFlowModel : BaseModel
    {
        public EnumWorkFlowType Type { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid? CsoId { get; set; }
        public Guid ClassLiveCalendarId { get; set; }
        public Guid? WorkFlowParentId { get; set; }
        public Guid LiveTimeFrameId { get; set; }
        public IList<DayOfWeek>? LiveDays { get; set; }
        public string? ClassName { get; set; }
        public string? ClassCode { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public string? TeacherName { get; set; }
        public double? StartTime { get; set; }
        public double? EndTime { get; set; }
        public Guid? CourseId { get; set; }
        public IList<ClassLiveWorkFlowPlan>? ClassLiveWorkFlowPlans { get; set; }

        public ClassLiveCalendarModel? ClassLiveCalendar { get; set; }
    }
}

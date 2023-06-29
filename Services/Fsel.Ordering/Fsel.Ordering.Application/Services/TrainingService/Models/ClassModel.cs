// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.TrainingService.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ClassModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EnumClassType Status { get; set; }
        public EnumTeacherApprovalStatus TeacherApprovalStatus { get; set; }
        public Guid CourseId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid? LiveTimeFrameId { get; set; }

        public IList<DayOfWeek>? LiveDays { get; set; }
    }
}

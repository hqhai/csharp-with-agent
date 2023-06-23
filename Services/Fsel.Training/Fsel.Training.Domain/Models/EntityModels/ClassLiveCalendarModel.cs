// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ClassLiveCalendarModel : BaseModel
    {
        public DateTime LiveDate { get; set; }
        public Guid LiveTimeFrameId { get; set; }
        public string? AccessLink { get; set; }
        public string? Note { get; set; }
        public EnumClassLiveCalendarStatus Status { get; set; }
        public Guid ClassId { get; set; }
        public string? TeacherName { get; set; }
        public string? ClassCode { get; set; }
        public string? ClassName { get; set; }
        public Guid? TeacherId { get; set; }
        public DateTime? EndTime { get; set; }

        public DateTime? StartTime { get; set; }

        public IList<DayOfWeek>? LiveDays { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
        public ClassModel? Class { get; set; }
        public IList<ClassStudentModel>? ClassStudents { get; set; }
    }
}
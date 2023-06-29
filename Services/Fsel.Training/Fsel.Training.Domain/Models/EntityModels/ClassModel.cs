// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ClassModel : BaseModel
    {
        public string? Code { get; set; }

        public string? Name { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartDate { get; set; }

        public EnumClassType Status { get; set; }

        public EnumTeacherApprovalStatus TeacherApprovalStatus { get; set; }

        public Guid CourseId { get; set; }

        public Guid PackageId { get; set; }
        public Guid? LiveTimeFrameId { get; set; }

        public IList<DayOfWeek>? LiveDays { get; set; }

        public Guid? TeacherId { get; set; }

        public string? TeacherName { get; set; }

        public EnumCourseLevel? CourseLevel { get; set; }

        public Guid? CsoId { get; set; }

        public IList<ClassStudentModel>? ClassStudents { get; set; }
        public IList<ClassLiveCalendarModel>? ClassLiveCalendars { get; set; }
    }
}

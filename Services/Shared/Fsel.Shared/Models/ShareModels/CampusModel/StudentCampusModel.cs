// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.CampusModel
{
    using System.ComponentModel;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentCampusModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Class { get; set; }
        public Guid StudentId { get; set; }
        public IList<StudentCampusLearningProgressModel>? Students { get; set; }
    }

    public class StudentCampusLearningProgressModel
    {
        public Guid StudentId { get; set; }
        public int TotalLessonDone { get; set; }
        public int TotalLesson { get; set; }
        public EnumStudentCampusLearningStatus ProgressStatus { get; set; }
        public string? LessonName { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public string? CourseName { get; set; }
        public string? CurriculumName { get; set; }
    }

    public enum EnumStudentCampusLearningStatus
    {
        [Description("Chưa triển khai")]
        NotStarted,

        [Description("Chưa vào học")]
        NotJoined,

        [Description("Đang học")]
        InProgress,

        [Description("Đã hoàn thành")]
        Completed
    }
}

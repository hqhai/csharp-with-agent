// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.CampusModel
{
    using System.ComponentModel;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentCampusModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Class { get; set; }
        public string? School { get; set; }
        public string? DefaultPassword { get; set; }
        public EnumGender? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public Guid StudentId { get; set; }
        public Guid? SchoolId { get; set; }
        public Guid? SchoolClassId { get; set; }
        public IList<StudentCampusLearningProgressModel>? LearningProgresses { get; set; }
    }

    public class StudentCampusLearningProgressModel
    {
        public Guid StudentId { get; set; }
        public int TotalLessonDone { get; set; }
        public int TotalLesson { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public EnumCurriculumStatus CurriculumStatus
        {
            get
            {
                var now = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
                if (now < StartDate)
                {
                    return EnumCurriculumStatus.NotProgress;
                }
                else if (now > EndDate)
                {
                    return EnumCurriculumStatus.Expired;
                }
                return EnumCurriculumStatus.Progress;
            }
        }

        public EnumStudentCampusLearningStatus Status { get; set; }

        public string? ProgressStatus
        {
            get
            {
                if (Status == EnumStudentCampusLearningStatus.NotStarted)
                {
                    return EnumStudentCampusLearningStatus.NotStarted.GetDescription();
                }
                else if (Status == EnumStudentCampusLearningStatus.InProgress)
                {
                    return $"{EnumStudentCampusLearningStatus.InProgress.GetDescription()} - {LessonName}";
                }
                else
                {
                    return EnumStudentCampusLearningStatus.Completed.GetDescription();
                }
            }
        }

        public string? LessonName { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public string? CourseName { get; set; }
        public string? CurriculumName { get; set; }
        public Guid CurriculumId { get; set; }
    }

    public enum EnumStudentCampusLearningStatus
    {
        [Description("Chưa triển khai")]
        NotStarted,

        [Description("Đang học")]
        InProgress,

        [Description("Đã hoàn thành")]
        Completed
    }
}

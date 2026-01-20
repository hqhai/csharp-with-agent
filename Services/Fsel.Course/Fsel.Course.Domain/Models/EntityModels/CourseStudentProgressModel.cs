// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class CourseStudentProgressModel
    {
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid CourseId { get; set; }
        public string? CourseName { get; set; }
        public int Visit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long TimeSpent { get; set; }
        public string? ContentCompleted { get; set; }
        public Guid ProgramId { get; set; }
        public string? Program { get; set; }
        public Guid LevelId { get; set; }
        public string? Level { get; set; }
        public Guid SubjectId { get; set; }
        public string? Subject { get; set; }
    }
}

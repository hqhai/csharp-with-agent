// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ExportEventModels
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class ExportStudentEventModelV2
    {
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? School { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public int NumberOfCompletedLessons { get; set; }
        public double OverrallScore { get; set; }
        public int NumberOfCompletedClassForum { get; set; }
        public int NumberOfCompletedComponent { get; set; }
        public int TotalComponent { get; set; }
        public int? NumberOfComments { get; set; }
        public string? LocalId { get; set; }
        public Guid? GlobalId { get; set; }
        public string? EventCode { get; set; }
    }
}

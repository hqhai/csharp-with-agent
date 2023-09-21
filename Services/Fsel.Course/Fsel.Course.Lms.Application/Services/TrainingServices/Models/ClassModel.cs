// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TrainingServices.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ClassModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public EnumTeacherApprovalStatus TeacherApprovalStatus { get; set; }
        public EnumStatusClass Status { get; set; }
        public Guid CourseId { get; set; }
        public Guid PackageId { get; set; }
        public IList<ClassStudentModel>? ClassStudents { get; set; }
    }
}

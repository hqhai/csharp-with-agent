// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.TrainingService.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ClassModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public EnumClassStatus Status { get; set; }
        public EnumTeacherApprovalStatus TeacherApprovalStatus { get; set; }
        public Guid CourseId { get; set; }
        public Guid PackageId { get; set; }
    }
}

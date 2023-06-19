// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using Fsel.Shared.Enums;

    public class ClassSearchModel
    {
        public string? ClassName { get; set; }
        public int NumberOfStudent { get; set; }
        public DateTime ExpectedDate { get; set; }
        public DateTime ActivationDate { get; set; }
        public int MaxScorePT { get; set; }
        public int AveragePT { get; set; }
        public string? CSOName { get; set; }
        public string? TeacherName { get; set; }
        public EnumClassType Status { get; set; }
        public string? PackageName { get; set; }
        public Guid PackageId { get; set; }
        public Guid CourseId { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid? CSOId { get; set; }

    }
}

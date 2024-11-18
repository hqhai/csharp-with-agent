// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Shared.Enums;

    public class StudentReportDashboardModel
    {
        public Guid Id { get; set; }
        public string? Membership { get; set; }
        public string? Occupation { get; set; }
        public string? School { get; set; }
        public string? ParentEmail { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumCourseLevel? BaseCourseLevel { get; set; }
        public Guid? PackageId { get; set; }
        public Guid? ClassId { get; set; }
        public double NumberOfToken { get; set; }
        public Guid? ProvinceId { get; set; }
        public bool IsDonePT { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SchoolId { get; set; }
        public string? SchoolClass { get; set; }
        public Guid? SenderId { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }
}

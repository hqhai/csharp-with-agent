// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Entities.BeginnerGuideConfigs;
    using Fsel.Shared.Enums;

    public class StudentModel : BaseModel
    {
        public Guid? PackageId { get; set; }
        public string? Occupation { get; set; }
        public string? School { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public HumanProfileModel? Human { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumCourseLevel? BaseCourseLevel { get; set; }
        public string? Membership { get; set; }
        public Guid? ClassId { get; set; }
        public string? CodeClass { get; set; }
        public string? EmailParent { get; set; }
        public string? ParentFullName { get; set; }
        public string? ParentEmail { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public long NumberOfToken { get; set; }
        public long NumberOfTokenReceived { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SchoolId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? SenderId { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public StudentBeginnerGuide? BeginnerGuide { get; set; }
        public ParentProfileModel? Parent { get; set; }
    }
}

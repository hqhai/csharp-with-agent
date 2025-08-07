// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    using System;
    using Fsel.Shared.Enums;

    public class EventRegistrationModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? TeacherPhoneNumber { get; set; }
        public string? ParentEmail { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public bool IsBussinessCheckBox { get; set; }
        public DateTime BirthDay { get; set; }
        public string? Province { get; set; }
        public Guid? ProvinceId { get; set; }
        public string? District { get; set; }
        public Guid? DistrictId { get; set; }
        public string? School { get; set; }
        public Guid? SchoolId { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public string? SchoolStudentCode { get; set; }
        public EnumEventRegistrationStatus Status { get; set; }
        public string? StudentMainMajor { get; set; }
        public Guid StudentId { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public Guid? CourseId { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
    }
}

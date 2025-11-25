// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;

    public class StudentModel : BaseModel
    {
        public string? Membership { get; set; }
        public string? Occupation { get; set; }
        public string? School { get; set; }
        public string? ParentFullName { get; set; }
        public string? ParentEmail { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumCourseLevel? BaseCourseLevel { get; set; }
        public Guid? PackageId { get; set; }
        public Guid? ClassId { get; set; }
        public string? SchoolClass { get; set; }
        public string? SchoolGrade { get; set; }
        public double NumberOfToken { get; set; }
        public long NumberOfTokenReceived { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? SchoolId { get; set; }
        public Guid? SchoolClassId { get; set; }
        public Guid? SenderId { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public Guid UserId { get; set; }
        public UserModel? User { get; set; }
        public StudentBeginnerGuideModel? BeginnerGuide { get; set; }
    }
}

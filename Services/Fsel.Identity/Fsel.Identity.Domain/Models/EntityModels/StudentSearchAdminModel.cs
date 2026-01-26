// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentSearchAdminModel : BaseModel
    {
        public string? FullName { get; set; }
        public string? StudentCode { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Email { get; set; }
        public EnumCourseType? Type { get; set; }
        public EnumGender? Gender { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public Guid? SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Grade { get; set; }
        public string? Class { get; set; }
        public string? UserName { get; set; }
        public string? PasswordDefault { get; set; }
        public string? Object { get; set; }
        public string? Status { get; set; }
        public string? Event { get; set; }
        public Guid? StudentId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public int? TotalLesson { get; set; }
        public int? TotalLessonDone { get; set; }
        public bool? EmailConfirm { get; set; }
        public bool IsLearnStudent { get; set; }
        public bool? IsDeleted { get; set; }
        public string? StudentCampusCode { get; set; }
        public Guid? ProgramId { get; set; }
        public string? Program { get; set; }
        public Guid? LevelId { get; set; }
        public string? Level { get; set; }
        public Guid? SubjectId { get; set; }
        public string? Subject { get; set; }
    }
}

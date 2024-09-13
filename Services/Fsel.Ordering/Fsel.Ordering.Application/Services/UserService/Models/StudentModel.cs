// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UserService.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentModel : BaseModel
    {
        public string? Membership { get; set; }
        public string? Occupation { get; set; }
        public string? School { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumCourseLevel? BaseCourseLevel { get; set; }
        public Guid? ClassId { get; set; }
        public long NumberOfToken { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SchoolId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? SenderId { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public Guid UserId { get; set; }
        public UserModel? User { get; set; }
    }
}

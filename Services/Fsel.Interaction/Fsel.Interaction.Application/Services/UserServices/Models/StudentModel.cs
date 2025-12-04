// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Services.UserServices.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentModel : BaseModel
    {
        public string? Code { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseLevel? BaseCourseLevel { get; set; }
        public string? FullName { get; set; }
        public string? AvatarPath { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SchoolId { get; set; }
        public Guid? UserId { get; set; }
        public UserModel? User { get; set; }
    }
}

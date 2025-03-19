// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.UserServices.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentModel : BaseModel
    {
        public Guid? PackageId { get; set; }
        public string? Occupation { get; set; }
        public string? School { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumCourseLevel? BaseCourseLevel { get; set; }
        public Guid? ClassId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? SchoolId { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public HumanModel? Human { get; set; }
    }
}

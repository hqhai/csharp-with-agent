// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Services.UserServices.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentInPlatformModel : BaseModel
    {
        public Guid StudentId { get; set; }
        public string? Code { get; set; }
        public string? UserName { get; set; }
        public EnumGameCourseLevel? CourseLevel { get; set; }
        public string? Status { get; set; }
        public string? Role { get; set; }
        public EnumUserPlatformStatus UserPlatformStatus { get; set; }
    }
}

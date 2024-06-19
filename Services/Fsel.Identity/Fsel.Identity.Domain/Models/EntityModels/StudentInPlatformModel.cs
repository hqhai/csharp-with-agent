// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Shared.Enums;

    public class StudentInPlatformModel : BaseModel
    {
        public Guid StudentId { get; set; }
        public string? Code { get; set; }
        public string? UserName { get; set; }
        public EnumGameCourseLevel? Level { get; set; }
        public string? Status { get; set; }
        public string? Role { get; set; }
        public EnumUserPlatformStatus UserPlatformStatus { get; set; }

    }
}

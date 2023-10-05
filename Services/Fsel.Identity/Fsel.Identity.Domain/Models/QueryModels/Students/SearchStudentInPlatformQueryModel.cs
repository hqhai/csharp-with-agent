// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.QueryModels.Students
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Enums;
    using Fsel.Shared.Enums;

    public class SearchStudentInPlatformQueryModel : BaseQueryModel
    {
        public EnumPlatformCode? PlatformCode { get; set; }
        public EnumRole? Role { get; set; }
        public EnumGameCourseLevel? Level { get; set; }
        public EnumUserPlatformStatus Status { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.StudentGameInfos
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchStudentsInPlatformQueryModel : BaseQueryModel
    {
        public EnumPlatformCode? PlatformCode { get; set; }
        public EnumRole? Role { get; set; }
        public EnumUserPlatformStatus UserPlatformStatus { get; set; }
        public EnumGameCourseLevel? Level { get; set; }
    }
}

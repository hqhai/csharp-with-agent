// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Services.UserServices.Models
{
    using Fsel.Shared.Enums;

    public class GetStudentInPlatformQueryModel
    {
        public EnumPlatformCode? PlatformCode { get; set; }
        public EnumRole? Role { get; set; }
        public EnumUserPlatformStatus Status { get; set; }
        public string? Keyword { get; set; }
    }
}

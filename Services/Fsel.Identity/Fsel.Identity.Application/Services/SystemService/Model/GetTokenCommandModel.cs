// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.Model
{
    using Fsel.Shared.Enums;

    public class GetTokenCommandModel
    {
        public EnumTokenFeature Feature { get; set; }
        public EnumTokenMission Mission { get; set; }
    }
}

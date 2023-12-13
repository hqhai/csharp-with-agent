// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.Model
{
    using Fsel.Shared.Enums;

    public class TokenConfigModel
    {
        public EnumTokenFeature Feature { get; set; }

        public EnumTokenMission Mission { get; set; }

        public object? Config { get; set; }

        public object? SuperConfig { get; set; }
    }
}

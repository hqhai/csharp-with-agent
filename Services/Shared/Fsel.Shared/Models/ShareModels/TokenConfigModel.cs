// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
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

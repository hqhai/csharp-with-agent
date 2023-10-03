// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.UserServices.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class PlatformModel : BaseModel
    {
        public EnumPlatformType Type { get; set; }

        public EnumPlatformCode Code { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }
    }
}

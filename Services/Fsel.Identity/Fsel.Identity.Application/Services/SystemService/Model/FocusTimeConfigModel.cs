// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.Model
{
    using Fsel.Core.Base.BaseModels;

    public class FocusTimeConfigModel : BaseModel
    {
        public double TargetTime { get; set; }

        public string? Description { get; set; }

        public int Token { get; set; }
    }
}

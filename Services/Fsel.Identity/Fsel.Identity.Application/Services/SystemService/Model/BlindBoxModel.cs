// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.Model
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class BlindBoxModel : BaseModel
    {
        public string? Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

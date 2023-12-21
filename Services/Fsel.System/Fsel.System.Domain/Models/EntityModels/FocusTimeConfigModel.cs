// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class FocusTimeConfigModel : BaseModel
    {
        public double TargetTime { get; set; }

        public string? Description { get; set; }

        public int TokenNumber { get; set; }
    }
}

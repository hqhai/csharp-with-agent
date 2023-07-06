// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class LiveTimeFrameModel : BaseModel
    {
        public double? StartTime { get; set; }

        public double? EndTime { get; set; }
    }
}

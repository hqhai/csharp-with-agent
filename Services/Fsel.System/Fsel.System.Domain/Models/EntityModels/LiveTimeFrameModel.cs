// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class LiveTimeFrameModel : BaseModel
    {
        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.SystemServices.Models
{
    using Fsel.Core.Base.BaseModels;

    public class LiveTimeFrameModel : BaseModel
    {
        public double? StartTime { get; set; }

        public double? EndTime { get; set; }
    }
}

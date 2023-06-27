// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.SystemServices.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class LiveTimeFrameModel : BaseModel
    {
        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.TrainingServices.Models
{
    using System;
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;

    public class ClassModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public EnumTrainingType Status { get; set; }
    }
}

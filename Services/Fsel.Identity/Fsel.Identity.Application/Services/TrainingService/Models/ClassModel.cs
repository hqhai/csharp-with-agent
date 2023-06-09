// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.TrainingService.Models
{
    using System;
    using Fsel.Shared.Enums;
    using Fsel.Core.Base.BaseModels;

    public class ClassModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
        public EnumClassType Status { get; set; }
        public Guid CourseId { get; set; }
    }
}

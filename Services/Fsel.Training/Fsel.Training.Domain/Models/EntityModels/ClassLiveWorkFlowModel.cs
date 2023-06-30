// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Training.Domain.Enums;

    public class ClassLiveWorkFlowModel : BaseModel
    {
        public EnumWorkFlow Type { get; set; }
        public EnumWorkFlowStatus Status { get; set; }
        public string? Description { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid? CsoId { get; set; }
        public Guid ClassLiveCalendarId { get; set; }
    }
}

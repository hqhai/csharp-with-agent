// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Training.Domain.Enums;

    public class ClassLiveWorkFlowModel : BaseModel
    {
        public EnumWorkFlowType Type { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid? CsoId { get; set; }
        public Guid ClassLiveCalendarId { get; set; }
        public Guid? WorkFlowParentId { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Training.Domain.Enums;

    public class AlternativeCalendarModel : BaseModel
    {
        public string? ClassName { get; set; }
        public string? TeacherName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? LiveDate { get; set; }
        public EnumWorkFlowStatus Status { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid? LiveTimeFrameId { get; set; }
    }
}

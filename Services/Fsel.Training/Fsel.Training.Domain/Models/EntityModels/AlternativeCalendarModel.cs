// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class ChangeTeacherLiveModel : BaseModel
    {
        public string? ClassName { get; set; }
        public string? TeacherName { get; set; }
        public double? StartTime { get; set; }
        public double? EndTime { get; set; }
        public DateTime? LiveDate { get; set; }
        public string? Status { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid? LiveTimeFrameId { get; set; }
        public Guid ClassLiveCalendarId { get; set; }
    }
}

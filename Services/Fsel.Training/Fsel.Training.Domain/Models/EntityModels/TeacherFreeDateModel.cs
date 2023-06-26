// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class TeacherFreeDateModel : BaseModel
    {
        public DateTime EndTime { get; set; }
        public DateTime StartTime { get; set; }
        public Guid TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public IList<TeacherFreeTimeModel>? TeacherFreeTimes { get; set; }
        public DateTime? TimeFrameEndTime { get; set; }
        public DateTime? TimeFrameStartTime { get; set; }
    }
}

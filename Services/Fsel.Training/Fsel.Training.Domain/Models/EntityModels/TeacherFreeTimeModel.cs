// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class TeacherFreeTimeModel : BaseModel
    {
        public Guid LiveTimeFrameId { get; set; }

        public Guid TeacherFreeDateId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public bool Priority { get; set; }
        public TeacherFreeDateModel? TeacherFreeDate { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime StartTime { get; set; }

        public Guid TeacherId { get; set; }

        public string? TeacherName { get; set; }

        public DateTime? TimeFrameEndTime { get; set; }
        public DateTime? TimeFrameStartTime { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class TeacherFreeTimeModel : BaseModel
    {
        public Guid LiveTimeFrameId { get; set; }

        public Guid TeacherFreeDateId { get; set; }

        public EnumDayOfWeek DayOfWeek { get; set; }

        public int Priority { get; set; }
        public TeacherFreeDateModel? TeacherFreeDate { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime StartTime { get; set; }

        public Guid TeacherId { get; set; }

        public string? TeacherName { get; set; }
    }
}

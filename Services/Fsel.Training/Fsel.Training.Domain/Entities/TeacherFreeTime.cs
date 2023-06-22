// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class TeacherFreeTime : Entity
    {
        public Guid LiveTimeFrameId { get; set; }

        public Guid TeacherFreeDateId { get; set; }

        public EnumDayOfWeek DayOfWeek { get; set; }

        public int Priority { get; set; }
        public TeacherFreeDate? TeacherFreeDate { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;

    public class TeacherFreeTime : Entity
    {
        public Guid LiveTimeFrameId { get; set; }

        public Guid TeacherFreeDateId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public bool Priority { get; set; }
        public TeacherFreeDate? TeacherFreeDate { get; set; }
    }
}

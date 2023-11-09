// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;

    public class TeacherFreeTimeLive : Entity
    {
        public DateTime LiveDate { get; set; }
        public bool IsUsed { get; set; }
        public Guid TeacherFreeTimeId { get; set; }
        public TeacherFreeTime? TeacherFreeTime { get; set; }
    }
}

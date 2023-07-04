// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.TeacherFreeTimes
{
    public class CreateTeacherFreeTimeCommandModel
    {
        public Guid LiveTimeFrameId { get; set; }
        public bool Priority { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
    }
}

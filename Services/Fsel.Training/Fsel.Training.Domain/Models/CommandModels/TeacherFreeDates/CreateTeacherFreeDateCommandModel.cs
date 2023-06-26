// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.TeacherFreeDates
{
    using Fsel.Training.Domain.Models.CommandModels.TeacherFreeTimes;

    public class CreateTeacherFreeDateCommandModel
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public IList<CreateTeacherFreeTimeCommandModel>? TeacherFreeTimes { get; set; }
    }
}

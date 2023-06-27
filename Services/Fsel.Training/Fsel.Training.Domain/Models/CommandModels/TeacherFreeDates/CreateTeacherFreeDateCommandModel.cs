// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.TeacherFreeDates
{
    using Fsel.Training.Domain.Models.CommandModels.TeacherFreeTimes;

    public class CreateTeacherFreeDateCommandModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IList<CreateTeacherFreeTimeCommandModel>? TeacherFreeTimes { get; set; }
    }
}

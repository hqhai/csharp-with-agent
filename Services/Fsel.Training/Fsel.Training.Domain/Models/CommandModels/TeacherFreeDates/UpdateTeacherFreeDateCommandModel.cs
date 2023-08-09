// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.TeacherFreeDates
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Training.Domain.Models.CommandModels.TeacherFreeTimes;

    public class UpdateTeacherFreeDateCommand : BaseCommandModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IList<CreateTeacherFreeTimeCommandModel>? TeacherFreeTimes { get; set; }
    }
}

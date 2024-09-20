// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.StudentFocusTime
{
    public class CreateStudentFocusTimeCommandModel
    {
        public double ExecuteTime { get; set; }

        public double TargetTime { get; set; }

        public Guid UserId { get; set; }
    }
}

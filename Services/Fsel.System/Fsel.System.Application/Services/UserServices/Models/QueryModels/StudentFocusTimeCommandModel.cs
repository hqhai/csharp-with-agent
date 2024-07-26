// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.UserServices.Models.QueryModels
{
    public class StudentFocusTimeCommandModel
    {
        public double ExecuteTime { get; set; }

        public double TargetTime { get; set; }

        public Guid UserId { get; set; }
    }
}

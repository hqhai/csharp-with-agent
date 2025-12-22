// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class StudentFocusTimeModel : BaseModel
    {
        public Guid StudentId { get; set; }

        public double ExecuteTime { get; set; }

        public double TargetTime { get; set; }

        public bool IsEstablished { get; set; }

        public bool IsReceivedToken { get; set; }

        public double? NumberOfToken { get; set; }

        public bool IsWeekStreak { get; set; }

        public double NearestTargetTime { get; set; }

        public bool IsFirstTimeInDay { get; set; }
    }
}

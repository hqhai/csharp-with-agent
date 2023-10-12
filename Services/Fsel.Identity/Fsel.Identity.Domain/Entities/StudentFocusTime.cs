// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Core.Entities;

    public class StudentFocusTime : Entity
    {
        public Guid StudentId { get; set; }

        public double ExecuteTime { get; set; }

        public double TargetTime { get; set; }

        public bool IsEstablished { get; set; }
    }
}

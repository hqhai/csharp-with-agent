// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Entities;

    public class TeacherFreeDate : Entity
    {
        /// <summary>
        /// End Date
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Start Date
        /// </summary>
        public DateTime StartDate { get; set; }

        public Guid TeacherId { get; set; }
        public ICollection<TeacherFreeTime> TeacherFreeTimes { get; set; } = new List<TeacherFreeTime>();
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Entities;

    public class TeacherFreeDate : Entity
    {
        /// <summary>
        /// End Time
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Start Time
        /// </summary>
        public DateTime StartTime { get; set; }

        public Guid TeacherId { get; set; }
    }
}

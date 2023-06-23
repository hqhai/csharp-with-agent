// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Training.Domain.Entities;

    public class TeacherFreeDateModel : BaseModel
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

        public string? TeacherName { get; set; }

        public IList<TeacherFreeTimeModel>? TeacherFreeTimes { get; set; }
    }
}

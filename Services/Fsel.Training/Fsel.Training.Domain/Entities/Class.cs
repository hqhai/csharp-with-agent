// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class Class : Entity
    {
        /// <summary>
        /// Id Class
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        /// <summary>
        /// Name Class
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// End Time
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Start Time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        public EnumClassType Status { get; set; }

        public Guid CourseId { get; set; }

        public Guid? LiveTimeFrameId { get; set; }

        public DayOfWeek LiveDays { get; set; }

        public Guid? TeacherId { get; set; }

        public Guid? CsoId { get; set; }

        public Guid? PackageId { get; set; }

        public ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();
        public ICollection<ClassLiveCalendar> ClassLiveCalendars { get; set; } = new List<ClassLiveCalendar>();
    }
}

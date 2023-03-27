// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using DataAnnotationsExtensions;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;

    public class UnitLessonResult : Entity
    {
        public long? Result { get; set; }

        [Min(1, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public long TimeWatchVideo { get; set; }

        public EnumResultStatus Status { get; set; }

        public UnitLesson? UnitLesson { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid UnitLessonId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }

        [NotMapped]
        public TimeSpan TimeWatchVideoSpan
        {
            get { return TimeSpan.FromTicks(TimeWatchVideo); }
        }
    }
}

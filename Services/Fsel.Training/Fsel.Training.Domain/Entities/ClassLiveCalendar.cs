// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Training.Domain.Enums;

    public class ClassLiveCalendar : Entity
    {
        public DateTime LiveDate { get; set; }

        public Guid LiveTimeFrameId { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? AccessLink { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Note { get; set; }

        public EnumClassLiveStatus Status { get; set; }

        public Guid ClassId { get; set; }

        public ICollection<Class>? Classes { get; set; } = new List<Class>();
    }
}

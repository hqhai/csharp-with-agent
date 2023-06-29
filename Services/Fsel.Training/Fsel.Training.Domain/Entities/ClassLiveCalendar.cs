// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class ClassLiveCalendar : Entity
    {
        public DateTime LiveDate { get; set; }
        public Guid LiveTimeFrameId { get; set; }
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? AccessLink { get; set; }
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Note { get; set; }
        public EnumClassLiveCalendarStatus Status { get; set; }
        public Guid ClassId { get; set; }

        public Class? Class { get; set; }
        public ICollection<ClassLiveWorkFlow> ClassLiveWorkFlows { get; set; } = new List<ClassLiveWorkFlow>();
    }
}

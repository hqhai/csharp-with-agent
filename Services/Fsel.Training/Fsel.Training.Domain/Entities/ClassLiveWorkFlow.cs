// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Training.Domain.Enums;

    public class ClassLiveWorkFlow : Entity
    {
        /// <summary>
        /// Luong
        /// </summary>
        public EnumWorkFlowType Type { get; set; }

        public string? Status { get; set; }

        /// <summary>
        /// Noi dung
        /// </summary>
        public string? Description { get; set; }

        public Guid? TeacherId { get; set; }
        public Guid? CsoId { get; set; }
        public ClassLiveCalendar? ClassLiveCalendar { get; set; }
        public Guid ClassLiveCalendarId { get; set; }
        public Guid? WorkFlowParentId { get; set; }
    }
}

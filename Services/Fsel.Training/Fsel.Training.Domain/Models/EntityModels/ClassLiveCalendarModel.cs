// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ClassLiveCalendarModel : BaseModel
    {
        public DateTime LiveDate { get; set; }
        public Guid LiveTimeFrameId { get; set; }
        public string? AccessLink { get; set; }
        public string? Note { get; set; }
        public EnumClassLiveCalendarStatus Status { get; set; }
        public Guid ClassId { get; set; }

        public ClassModel? Class { get; set; }
    }
}

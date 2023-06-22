// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.Classes
{
    using Fsel.Shared.Enums;

    public class ChangeStatusClassCommandModel
    {
        public Guid ClassId { get; set; }
        public EnumClassType Status { get; set; }

        public Guid LiveTimeFrameId { get; set; }
        public IList<DayOfWeek> LiveDays { get; set; }
    }
}

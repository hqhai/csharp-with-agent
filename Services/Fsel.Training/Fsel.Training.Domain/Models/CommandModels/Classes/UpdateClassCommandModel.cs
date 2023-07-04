// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.Classes
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateClassCommandModel : BaseCommandModel
    {
        public Guid? LiveTimeFrameId { get; set; }
        public IList<DayOfWeek>? LiveDays { get; set; }
    }
}

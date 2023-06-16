// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.CourseTimeConfigs
{
    using Fsel.Core.Base.BaseModels;

    public class SetMonthToClassCommandModel : BaseCommandModel
    {
        public int DurationMonth { get; set; }
    }
}

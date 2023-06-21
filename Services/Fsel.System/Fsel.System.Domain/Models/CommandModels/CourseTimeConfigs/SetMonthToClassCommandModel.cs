// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.CourseTimeConfigs
{
    public class SetMonthToClassCommandModel
    {
        public int DurationMonth { get; set; }

        public Guid CourseId { get; set; }
    }
}

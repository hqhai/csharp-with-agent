// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.CourseTimeConfigs
{
    public class SetListEnrollmentWeekToClassCommandModel
    {
        public IList<SetEnrollmentWeekToClassCommandModel>? CourseTimeConfigs { get; set; }
    }
}

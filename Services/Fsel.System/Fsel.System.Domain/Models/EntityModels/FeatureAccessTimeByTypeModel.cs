// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{

    public class FeatureAccessTimeByTypeModel
    {
        public long AccessTime { get; set; }

        public int HourActive { get; set; }

        public Guid? CourseId { get; set; }

        public DayOfWeek DayActive { get; set; }

        public long TotalHourActive { get; set; }

        public int MonthActive { get; set; }
    }
}

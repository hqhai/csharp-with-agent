// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    public class FeatureAccessTimeByTypeModel
    {
        public long AccessTime { get; set; }

        public int HourActive { get; set; }

        public Guid? CourseId { get; set; }

        public DayOfWeek DayActive { get; set; }
    }
}

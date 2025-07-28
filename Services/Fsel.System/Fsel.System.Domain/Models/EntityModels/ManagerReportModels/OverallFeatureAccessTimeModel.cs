// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels.ManagerReportModels
{
    public class OverallFeatureAccessTimeModel
    {
        public Guid CourseId { get; set; }
        public Guid UserId { get; set; }
        public long TotalTimeVideo { get; set; }
        public long TotalTimeHomeWork { get; set; }
        public long TotalTimeClassForum { get; set; }
        public long TotalTime { get; set; }
        public long TotalVisit { get; set; }
        public DateTime? CurrentDate { get; set; }
    }
}

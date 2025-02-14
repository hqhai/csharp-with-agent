// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    public class FeatureAccessTimesQueryModel
    {
        public IList<FeatureAccessTimeQueryModel> FeatureAccessTimes { get; set; } = new List<FeatureAccessTimeQueryModel>();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid UserId { get; set; }
    }
}

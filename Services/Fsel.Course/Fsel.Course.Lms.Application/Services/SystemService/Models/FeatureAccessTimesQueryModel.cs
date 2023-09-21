// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    public class FeatureAccessTimesQueryModel
    {
        public IList<FeatureAccessTimeQueryModel>? FeatureAccessTimes { get; set; }
        public Guid UserId { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    public class GetFeatureAccessTimesQueryModel
    {
        public IList<GetFeatureAccessTimeQueryModel> FeatureAccessTimes { get; set; } = new List<GetFeatureAccessTimeQueryModel>();
        public Guid UserId { get; set; }
    }
}

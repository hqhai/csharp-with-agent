// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.CourseServices.QueryModels
{
    using Fsel.System.Domain.Enums;

    public class GetAICriteriaConfigsQueryModel
    {
        public Guid? Id { get; set; }
        public Guid? ObjectId { get; set; }
        public IList<Guid>? ObjectIds { get; set; }
        public EnumSubFeatureType SubFeatureType { get; set; }
        public EnumFeatureMultiple FeatureMultiple { get; set; }
    }
}

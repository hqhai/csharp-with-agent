// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    public class GetFeatureAccessTimesByCourseIdsQueryModel
    {
        public IList<Guid> CourseIds { get; set; } = new List<Guid>();
        public Guid UserId { get; set; }
    }
}

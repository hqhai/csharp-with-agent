// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    public class FeatureAccessTimesByMockTestIdQueryModel
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid ObjectId { get; set; }
        public Guid UserId { get; set; }
    }
}

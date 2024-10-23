// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    using global::System;

    public class GetFeatureAccessTimeBusinessQueryModel
    {
        public Guid UserId { get; set; }
        public Guid? CourseId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

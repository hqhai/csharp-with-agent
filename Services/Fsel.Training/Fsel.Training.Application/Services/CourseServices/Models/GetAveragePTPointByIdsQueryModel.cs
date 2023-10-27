// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.CourseServices.Models
{
    using System;
    using System.Collections.Generic;

    public class GetAveragePTPointByIdsQueryModel
    {
        public IList<GetAveragePTPointByIdQueryModel> PointByIdQueryModels { get; set; } = new List<GetAveragePTPointByIdQueryModel>();
    }

    public class GetAveragePTPointByIdQueryModel
    {
        public Guid ClassId { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }
}

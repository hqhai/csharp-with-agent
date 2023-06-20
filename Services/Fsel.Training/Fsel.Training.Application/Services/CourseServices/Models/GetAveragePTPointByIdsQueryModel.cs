// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.CourseServices.Models
{
    using System;
    using System.Collections.Generic;

    public class GetAveragePTPointByIdsQueryModel
    {
        public Guid ClassId { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }
}

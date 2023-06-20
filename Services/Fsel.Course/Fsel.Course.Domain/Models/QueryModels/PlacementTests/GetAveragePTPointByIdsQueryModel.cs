// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.PlacementTests
{
    using System;

    public class GetAveragePTPointByIdsQueryModel
    {
        public Guid ClassId { get; set; }
        public IList<Guid>? StudentIds { get; set;}
    }
}

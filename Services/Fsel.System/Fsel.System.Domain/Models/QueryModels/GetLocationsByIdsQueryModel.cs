// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    using global::System;
    using global::System.Collections.Generic;

    public class GetLocationsByIdsQueryModel
    {
        public IList<Guid>? Ids { get; set; }
    }
}

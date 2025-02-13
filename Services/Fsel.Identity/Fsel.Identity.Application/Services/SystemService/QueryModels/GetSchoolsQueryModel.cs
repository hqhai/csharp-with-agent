// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.QueryModels
{
    using System;
    using System.Collections.Generic;

    public class GetSchoolsQueryModel
    {
        public IList<Guid>? SchoolIds { get; set; }
        public IList<Guid>? ProvinceIds { get; set; }
        public IList<Guid>? DistrictIds { get; set; }
    }
}

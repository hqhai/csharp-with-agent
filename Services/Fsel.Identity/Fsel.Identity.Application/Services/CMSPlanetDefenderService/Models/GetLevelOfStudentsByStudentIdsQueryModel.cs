// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.CMSPlanetDefenderService.Models
{
    using System;
    using System.Collections.Generic;

    public class GetLevelOfStudentsByStudentIdsQueryModel
    {
        public IList<Guid>? StudentIds { get; set; }
    }
}

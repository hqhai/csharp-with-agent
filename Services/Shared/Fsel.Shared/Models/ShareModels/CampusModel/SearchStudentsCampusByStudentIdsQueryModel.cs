// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels.CampusModel
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;

    public class SearchStudentsCampusByStudentIdsQueryModel : BaseQueryModel
    {
        public IList<Guid>? SchoolClassIds { get; set; }
        public IList<Guid>? StudentIds { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.PlacementTestSections
{
    using System.Collections.Generic;
    using Fsel.Course.Domain.Models.CommandModels.SectionGroups;

    public class UpdatePlacementTestSectionCommandModel
    {
        public IList<UpdateSectionGroupCommandModel>? SectionGroups { get; set; }
    }
}

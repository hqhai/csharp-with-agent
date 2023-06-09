// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.PlacementTestSections
{
    using Fsel.Course.Domain.Models.CommandModels.SectionGroups;

    public class UpdatePlacementTestCommandModel
    {
        public IList<UpdateSectionGroupCommandModel>? SectionGroups { get; set; }
    }
}

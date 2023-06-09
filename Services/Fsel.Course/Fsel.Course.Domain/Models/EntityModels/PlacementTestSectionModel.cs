// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class PlacementTestSectionModel
    {
        public Guid PlacementTestId { get; set; }
        public Guid SectionGroupId { get; set; }
        public SectionGroupModel? SectionGroup { get; set; }
    }
}

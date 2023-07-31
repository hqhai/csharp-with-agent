// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class PlacementTestModel : BaseModel
    {
        public string? Name { get; set; }

        public string? InstructionContent { get; set; }

        public bool IsActive { get; set; }

        public EnumPlacementTestLevel Level { get; set; }
        public IList<SectionGroupModel>? SectionGroups { get; set; }
    }
}

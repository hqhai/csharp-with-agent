// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Models.CommandModels.SectionGroups;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.PlacementTests
{
    public class CreatePlacementTestCommandModel
    {
        public string? Name { get; set; }

        public string? InstructionContent { get; set; }

        public bool IsActive { get; set; }

        public EnumPlacementTestLevel Level { get; set; }
        public IList<CreateSectionGroupCommandModel>? SectionGroups { get; set; }
    }
}

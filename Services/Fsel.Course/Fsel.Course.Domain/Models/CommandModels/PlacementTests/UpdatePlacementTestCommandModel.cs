// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Models.CommandModels.SectionGroups;

namespace Fsel.Course.Domain.Models.CommandModels.PlacementTests
{
    public class UpdatePlacementTestCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public string? InstructionContent { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
        public IList<UpdateSectionGroupCommandModel>? SectionGroups { get; set; }
    }
}

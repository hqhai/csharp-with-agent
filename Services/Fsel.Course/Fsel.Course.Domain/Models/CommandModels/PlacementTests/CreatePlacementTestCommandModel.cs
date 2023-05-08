// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Models.CommandModels.PlacementTestSections;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.PlacementTests
{
    public class CreatePlacementTestCommandModel
    {
        public string? Name { get; set; }

        public string? InstructionContent { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
        public IList<CreatePlacementTestSectionCommandModel>? PlacementTestSections { get; set; }
    }
}

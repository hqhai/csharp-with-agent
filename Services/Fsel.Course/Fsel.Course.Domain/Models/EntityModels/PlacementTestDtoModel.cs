// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class PlacementTestDtoModel : BaseModel
    {
        public string? Name { get; set; }
        public string? InstructionContent { get; set; }
        public bool IsActive { get; set; }
        public double ExecutionTime { get; set; }
        public double TotalQuestion { get; set; }
        public EnumPlacementTestLevel Level { get; set; }
        public IList<EnumCourseSkill>? CourseSkills { get; set; }

        public PlacementTestResultModel? PlacementTestResult { get; set; }
        public IList<SectionGroupModel>? SectionGroups { get; set; }
    }
}

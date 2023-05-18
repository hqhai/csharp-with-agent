// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class PlacementTestBankModel
    {
        public EnumPlacementTestLevel Level { get; set; }
        public IList<SectionGroupModel>? SectionGroups { get; set; }
        public IList<PlacementTestBankSkillModel>? PlacementSkills { get; set; }
    }
    public class PlacementTestBankSkillModel
    {
        public EnumCourseSkill CourseSkill { get; set; }
        public IList<SectionModel>? Sections { get; set; }
    }

}

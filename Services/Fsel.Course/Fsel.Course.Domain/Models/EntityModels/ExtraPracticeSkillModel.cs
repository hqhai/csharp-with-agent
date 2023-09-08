// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ExtraPracticeSkillModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? InstructionContent { get; set; }
        public bool IsActive { get; set; }
        public EnumExtraPracticeType Type { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public IList<EnumCourseSkill>? CourseSkills { get; set; }
    }
}

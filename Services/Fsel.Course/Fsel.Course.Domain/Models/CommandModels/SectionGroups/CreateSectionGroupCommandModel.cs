// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.SectionGroups
{
    using Fsel.Course.Domain.Models.CommandModels.Sections;
    using Fsel.Shared.Enums;

    public class CreateSectionGroupCommandModel
    {
        public double ExecutionTime { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public IList<CreateSectionCommandModel>? Sections { get; set; }
    }
}

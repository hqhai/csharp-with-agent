// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.SectionGroups
{
    using Fsel.Course.Domain.Models.CommandModels.Sections;
    using Fsel.Shared.Enums;

    public class UpdateSectionGroupCommandModel
    {
        public double ExecutionTime { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public IList<UpdateSectionCommandModel>? Sections { get; set; }
    }
}

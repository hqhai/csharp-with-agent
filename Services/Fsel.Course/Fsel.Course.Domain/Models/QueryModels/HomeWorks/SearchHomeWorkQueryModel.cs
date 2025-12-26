// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.HomeWorks
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class SearchHomeWorkQueryModel : BaseQueryModel
    {
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumHomeWorkType Type { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public Guid? SkillId { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? LevelId { get; set; }
        public Guid? OriginalId { get; set; }
    }
}

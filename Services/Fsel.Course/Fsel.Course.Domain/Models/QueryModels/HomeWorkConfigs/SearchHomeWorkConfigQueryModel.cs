// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.HomeWorkConfigs
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchHomeWorkConfigQueryModel : BaseQueryModel
    {
        public Guid CurriculumId { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }
        public IList<EnumCourseSkill>? CourseSkills { get; set; }
        public IList<EnumCourseType>? CourseTypes { get; set; }
        public IList<Guid>? CreatedUserIds { get; set; }
    }
}

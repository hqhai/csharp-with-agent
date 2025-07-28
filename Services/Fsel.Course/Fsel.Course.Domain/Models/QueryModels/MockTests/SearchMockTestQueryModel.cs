// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.MockTests
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class SearchMockTestQueryModel : BaseQueryModel
    {
        public EnumMockTestType? MockTestType { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }
        public Guid? SkillId { get; set; }
    }
}

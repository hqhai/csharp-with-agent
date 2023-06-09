// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ExtraPractices
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class SearchExtraPracticeLmsQueryModel : BaseQueryModel
    {
        public IList<Guid>? UnitIds { get; set; }
        public IList<EnumCourseSkill>? CourseSkills { get; set; }
        public IList<EnumExtraPracticeProgress>? Progresses { get; set; }
        public IList<EnumExtraPracticeType>? Types { get; set; }
    }
}

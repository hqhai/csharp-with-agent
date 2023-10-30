// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SectionGroupDetailModel : BaseModel
    {
        public double ExecutionTime { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public long TotalQuestion { get; set; }
        public IList<SectionDetailModel>? Sections { get; set; }
        public SectionGroupResultModel? SectionGroupResult { get; set; }
    }
}

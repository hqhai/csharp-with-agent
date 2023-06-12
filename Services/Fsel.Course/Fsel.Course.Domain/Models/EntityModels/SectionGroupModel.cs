// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SectionGroupModel : BaseModel
    {
        public long ExecutionTime { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public IList<SectionModel>? Sections { get; set; }

        public long TotalQuestion { get; set; }
    }
}

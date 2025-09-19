// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class HomeWorkSearchModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }
        public EnumHomeWorkType Type { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
    }
}

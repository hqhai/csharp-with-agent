// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Collections.Generic;
    using Fsel.Shared.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;

    public class HomeWorkSearchModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public IList<LessonHomeWork>? LessonHomeWorks { get; set; }
    }
}

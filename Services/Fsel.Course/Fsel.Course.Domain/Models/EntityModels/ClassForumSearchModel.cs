// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ClassForumSearchModel : BaseModel
    {
        public EnumGradingStyle GradingStyle { get; set; }

        public long TaggetWordLimit { get; set; }
        public long TaggetTimeLimit { get; set; }

        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public Guid? LessonId { get; set; }

        public LessonModel? Lesson { get; set; }

        public string? LessonName { get; set; }

        public string? UnitName { get; set; }

        public Guid? TeacherId { get; set; }

        public IList<ClassForumFileModel>? ClassForumFiles { get; set; }

        public ClassForumResultModel? ClassForumResult { get; set; }
    }
}

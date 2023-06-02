// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class ClassForumModel : BaseModel
    {
        public EnumGradingStyle GradingStyle { get; set; }

        public long TaggetWordLimit { get; set; }
        public long TaggetTimeLimit { get; set; }

        public string? MediaPost { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public Guid? LessonId { get; set; }

        public LessonModel? Lesson { get; set; }

        public IList<ClassForumFileModel>? ClassForumFiles { get; set; }

        public IList<ClassForumResultModel>? ClassForumResults { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ClassForumReportModel : BaseModel
    {
        public string? Title { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public ClassForumResultScoreModel? ClassForumResultScore { get; set; }
    }
}

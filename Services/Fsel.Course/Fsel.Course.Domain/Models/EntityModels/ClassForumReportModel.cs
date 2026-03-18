// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ClassForumReportModel : BaseModel
    {
        public EnumCourseSkill CourseSkill { get; set; }
        public string? SkillName { get; set; }
        public string? SkillFilePath { get; set; }
        public Guid SkillId { get; set; }

        public string? Name { get; set; }
        public Guid LessonId { get; set; }
        public Guid? LessonResultId { get; set; }
        public EnumGradingStyle GradingStyle { get; set; }
        public ClassForumResultScoreModel? ClassForumResultScore { get; set; }
    }
}

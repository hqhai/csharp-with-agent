// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class ExerciseModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
        public Guid? SkillId { get; set; }
        public string? SkillName { get; set; }
        public string? MediaPostContent => StringHelper.ProcessHtml(MediaPost, true);
        public string? MediaPostContentRuby { get; set; }
        public IEnumerable<string>? AudioPaths => StringHelper.GetIframeUrls(MediaPost, true);
        public IEnumerable<string>? VideoPaths => StringHelper.GetIframeUrls(MediaPost, false);
        public EnumCourseSkill CourseSkill { get; set; }
        public SkillViewModel? Skill { get; set; }
        public IList<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
    }
}

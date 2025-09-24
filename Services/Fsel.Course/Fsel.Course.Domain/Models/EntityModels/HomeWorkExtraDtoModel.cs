// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class HomeWorkExtraDtoModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? MediaPost { get; set; }
        public EnumHomeWorkType Type { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumCourseSkill CourseSkill { get; set; }
        public string? MediaPostContent => StringHelper.ProcessHtml(MediaPost, false);
        public IEnumerable<string>? AudioPaths => StringHelper.GetIframeUrls(MediaPost, true);
        public IEnumerable<string>? VideoPaths => StringHelper.GetIframeUrls(MediaPost, false);
        public IList<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
        public HomeWorkExtraPracticeResultModel? HomeWorkExtraPracticeResult { get; set; }
    }
}

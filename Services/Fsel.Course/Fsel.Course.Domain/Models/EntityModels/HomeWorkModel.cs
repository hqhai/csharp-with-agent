// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class HomeWorkModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }
        public EnumHomeWorkType Type { get; set; }
        public string? MediaPost { get; set; }
        public string? MediaPostContent => StringHelper.ProcessHtml(MediaPost, false);

        public IEnumerable<string>? AudioPaths => StringHelper.GetIframeUrls(MediaPost, true);

        public IEnumerable<string>? VideoPaths => StringHelper.GetIframeUrls(MediaPost, false);

        public IList<QuestionModel> Questions { get; set; } = new List<QuestionModel>();

        public bool IsActive { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }

        public HomeWorkResultModel? HomeWorkResult { get; set; }
    }
}

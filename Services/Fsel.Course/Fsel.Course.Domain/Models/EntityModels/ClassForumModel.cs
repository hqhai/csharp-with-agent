// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Fsel.Shared.Enums;
using Fsel.Shared.Helpers;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class ClassForumModel : BaseModel
    {
        public EnumGradingStyle GradingStyle { get; set; }
        public string? PromptName { get; set; }
        public long TaggetWordLimit { get; set; }
        public double TaggetTimeLimit { get; set; }

        public string? MediaPost { get; set; }

        public string? MediaPostContent => StringHelper.ProcessHtml(MediaPost, false);

        public IEnumerable<string>? AudioPaths => StringHelper.GetIframeUrls(MediaPost, true);

        public IEnumerable<string>? VideoPaths => StringHelper.GetIframeUrls(MediaPost, false);

        public EnumCourseSkill CourseSkill { get; set; }
        public Guid? SkillId { get; set; }
        public string? SkillName { get; set; }

        public Guid? LessonId { get; set; }
        public bool IsAlFeedBack { get; set; }

        public string? SystemRoleAlConfig { get; set; }

        public string? UserAlConfig { get; set; }

        public string? SettingModel { get; set; }

        public double SettingTemperature { get; set; }

        public double SettingWordMaxLength { get; set; }

        public double SettingTopP { get; set; }

        public double SettingFrequecy { get; set; }

        public double SettingPresence { get; set; }

        public IList<ClassForumFileModel>? ClassForumFiles { get; set; }

        public IList<ClassForumResultModel>? ClassForumResults { get; set; }
    }
}

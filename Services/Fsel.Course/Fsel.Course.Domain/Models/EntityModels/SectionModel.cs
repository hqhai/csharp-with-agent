// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Helpers;

    public class SectionModel : BaseModel
    {
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
        public string? MediaPostContent => StringHelper.ProcessHtml(MediaPost, false);

        public IEnumerable<string>? AudioPaths => StringHelper.GetIframeUrls(MediaPost, true);

        public IEnumerable<string>? VideoPaths => StringHelper.GetIframeUrls(MediaPost, false);
        public int TargetWord { get; set; }
        public string? VideoFilePath { get; set; }
        public string? SubFilePath { get; set; }
        public int DisplayOrder { get; set; }
        public IList<SectionPartModel>? SectionParts { get; set; }
        public IList<SectionQuestionModel>? SectionQuestions { get; set; }
        public IList<SectionTimeCodeModel>? SectionTimeCodes { get; set; }
        public IList<QuestionModel>? Questions { get; set; }
        public MockTestAnswerModel? MockTestAnswer { get; set; }
        public object? Answer { get; set; }
    }
}

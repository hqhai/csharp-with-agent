// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;
    using StringHelper = Shared.Helpers.StringHelper;

    public class SectionDtoModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
        public string? MediaPostContent => StringHelper.ProcessHtml(MediaPost, false);

        public IEnumerable<string>? AudioPaths => StringHelper.GetIframeUrls(MediaPost, true).AddS3BaseUrls();

        public IEnumerable<string>? VideoPaths => StringHelper.GetIframeUrls(MediaPost, false).AddS3BaseUrls();
        public int TargetWord { get; set; }
        private string? _videoFilePath;

        public string? VideoFilePath
        {
            set { _videoFilePath = value; }
            get { return _videoFilePath.AddS3BaseUrl(); }
        }

        private string? _subFilePath;

        public string? SubFilePath
        {
            set { _subFilePath = value; }
            get { return _subFilePath.AddS3BaseUrl(); }
        }

        public int DisplayOrder { get; set; }
        public IList<SectionPartDtoModel>? SectionParts { get; set; }
        public IList<SectionTimeCodeDtoModel>? SectionTimeCodes { get; set; }
        public IList<Guid>? QuestionIds { get; set; }
        public int TotalQuestion { get; set; }
        public int CountQuestion { get; set; }
        public IList<QuestionCorrectStatusModel>? QuestionTests { get; set; }
        public MockTestAnswerModel? MockTestAnswer { get; set; }
        public object? Answer { get; set; }
    }
}

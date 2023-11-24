// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Helpers;

    public class SectionDtoModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
        public string? MediaPostContent => StringHelper.ProcessHtml(MediaPost, false);

        public IEnumerable<string>? AudioPaths => StringHelper.GetIframeUrls(MediaPost, true);

        public IEnumerable<string>? VideoPaths => StringHelper.GetIframeUrls(MediaPost, false);
        public int TargetWord { get; set; }
        public string? VideoFilePath { get; set; }
        public string? SubFilePath { get; set; }
        public int DisplayOrder { get; set; }
        public IList<SectionPartDtoModel>? SectionParts { get; set; }
        public IList<SectionTimeCodeDtoModel>? SectionTimeCodes { get; set; }
        public IList<Guid>? QuestionIds { get; set; }
        public object? Answer { get; set; }
    }
}

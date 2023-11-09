// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class SectionDtoModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
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

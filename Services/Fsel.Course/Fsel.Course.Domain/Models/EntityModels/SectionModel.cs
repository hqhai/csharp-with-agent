// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class SectionModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
        public int TargetWord { get; set; }
        public string? VideoFilePath { get; set; }
        public int DisplayOrder { get; set; }
        public Guid SectionGroupId { get; set; }
        public IList<SectionPartModel>? SectionParts { get; set; }

        public IList<SectionTimeCodeModel>? SectionTimeCodes { get; set; }
        public IList<QuestionModel>? Questions { get; set; }
    }
}

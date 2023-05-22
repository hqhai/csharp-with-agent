// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Sections
{
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Course.Domain.Models.CommandModels.SectionParts;
    using Fsel.Course.Domain.Models.CommandModels.SectionTimeCodes;

    public class CreateSectionCommandModel
    {
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
        public int TargetWord { get; set; }
        public int DisplayOrder { get; set; }
        public string? VideoFilePath { get; set; }
        public IList<CreateSectionPartCommandModel>? SectionParts { get; set; }
        public IList<CreateQuestionCommandModel>? Questions { get; set; }
        public IList<CreateSectionTimeCodeCommandModel>? SectionTimeCodes { get; set; }
    }
}

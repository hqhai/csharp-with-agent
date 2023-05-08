// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Sections
{
    using Fsel.Course.Domain.Models.CommandModels.SectionPartQuestions;
    using Fsel.Course.Domain.Models.CommandModels.SectionQuestions;

    public class CreateSectionCommandModel
    {
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
        public int TargetWord { get; set; }
        public string? VideoFilePath { get; set; }
        public IList<CreateSectionPartCommandModel>? SectionParts { get; set; }
        public IList<CreateSectionQuestionCommandModel>? SectionQuestions { get; set; }
    }
}

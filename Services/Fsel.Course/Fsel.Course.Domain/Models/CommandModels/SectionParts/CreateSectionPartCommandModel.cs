// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.SectionPartQuestions
{
    using Fsel.Course.Domain.Models.CommandModels.SectionQuestions;

    public class CreateSectionPartCommandModel
    {
        public string? PartName { get; set; }
        public IList<CreateSectionQuestionCommandModel>? SectionQuestions { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.SectionParts
{
    using Fsel.Course.Domain.Models.CommandModels.Questions;

    public class CreateSectionPartCommandModel
    {
        public string? PartName { get; set; }
        public IList<CreateQuestionCommandModel>? Questions { get; set; }
    }
}

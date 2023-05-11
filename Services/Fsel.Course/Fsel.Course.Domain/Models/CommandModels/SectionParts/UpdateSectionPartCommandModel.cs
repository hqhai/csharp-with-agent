// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.SectionParts
{
    using Fsel.Course.Domain.Models.CommandModels.Questions;

    public class UpdateSectionPartCommandModel
    {
        public string? PartName { get; set; }
        public IList<UpdateQuestionCommandModel>? Questions { get; set; }
    }
}

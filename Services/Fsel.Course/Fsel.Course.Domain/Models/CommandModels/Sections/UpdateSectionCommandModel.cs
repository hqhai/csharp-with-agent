// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Sections
{
    using Fsel.Course.Domain.Models.CommandModels.Questions;
    using Fsel.Course.Domain.Models.CommandModels.SectionParts;

    public class UpdateSectionCommandModel
    {
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
        public int TargetWord { get; set; }
        public string? VideoFilePath { get; set; }
        public IList<UpdateSectionPartCommandModel>? SectionParts { get; set; }
        public IList<UpdateQuestionCommandModel>? Questions { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class SectionModel : BaseModel
    {
        public string? Name { get; set; }
        public string? MediaPost { get; set; }
        public int TargetWord { get; set; }
        public Guid SectionGroupId { get; set; }
        public string? VideoFilePath { get; set; }
        public IList<SectionPartModel>? SectionParts { get; set; }
        public IList<SectionQuestionModel>? SectionQuestions { get; set; }
        public IList<SectionTimeCodeModel>? SectionTimeCodes { get; set; }
        public IList<QuestionModel>? Questions { get; set; }
    }
}

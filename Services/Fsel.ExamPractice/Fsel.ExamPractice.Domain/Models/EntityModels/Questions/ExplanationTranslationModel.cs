// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.Questions
{
    using Fsel.Core.Base.Interfaces;

    public class ExplanationTranslationModel : ITranslationObject
    {
        public string? Language { get; set; }
        public IList<string>? Contents { get; set; }
    }
}

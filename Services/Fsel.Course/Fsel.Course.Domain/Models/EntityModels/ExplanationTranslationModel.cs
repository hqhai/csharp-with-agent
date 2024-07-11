// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.Interfaces;

    public class ExplanationTranslationModel : ITranslationObject
    {
        public string? Language { get; set; }
        public IList<string>? Contents { get; set; }
    }
}

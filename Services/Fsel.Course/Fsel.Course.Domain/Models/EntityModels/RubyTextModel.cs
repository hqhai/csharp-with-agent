// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class RubyTextModel
    {
        public Guid Id { get; set; }
        public EnumLanguageType LanguageType { get; set; }
        public string? BaseText { get; set; }
        public string? Phonetic { get; set; }
    }
}

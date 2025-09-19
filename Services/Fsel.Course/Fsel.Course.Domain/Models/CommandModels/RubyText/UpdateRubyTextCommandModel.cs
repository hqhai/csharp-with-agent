// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.RubyText
{
    using Fsel.Shared.Enums;

    public class UpdateRubyTextCommandModel
    {
        public Guid Id { get; set; }
        public EnumLanguageType LangueType { get; set; }
        public string? BaseText { get; set; }
        public string? Phonetic { get; set; }
    }
}

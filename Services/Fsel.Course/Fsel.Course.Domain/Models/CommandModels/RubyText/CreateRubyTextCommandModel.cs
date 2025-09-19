// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.RubyText
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CreateRubyTextCommandModel : BaseCommandModel
    {
        public EnumLanguageType LangueType { get; set; }
        public string? BaseText { get; set; }
        public string? Phonetic { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.Ruby
{
    using System;
    using Fsel.Shared.Enums;

    public class RubyAnnotaionModel
    {
        public Guid Id { get; set; }
        public int StartGraphemeIndex { get; set; }
        public int LengthGraphemes { get; set; }
        public string? SelectedText { get; set; }
        public string? Phonetic { get; set; }
        public EnumLanguageType LanguageType { get; set; }
        public byte Position { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.RubyAnnotation
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CreateRubyAnnotationCommandModel : BaseCommandModel
    {
        public Guid HostId { get; set; }
        public string? HostType { get; set; }
        public string? FieldKey { get; set; }
        public string? Text { get; set; }
        public EnumLanguageType LangueType { get; set; }
        public string? SelectedText { get; set; }
        public string? Phonetic { get; set; }
        public int StartGraphemeIndex { get; set; }
        public int LengthGraphemes { get; set; }
        public string? PrefixContext { get; set; }
        public string? SuffixContext { get; set; }
        public RubyReturn RubyReturn { get; set; } = new();
    }

    public sealed class RubyReturn
    {
        public bool Html { get; set; } = true;
        public bool Annotations { get; set; }
    }
}

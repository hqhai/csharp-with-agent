// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.RubyAnnotation
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UpdateRubyAnnotationCommandModel : BaseCommandModel
    {
        public Guid Id { get; set; }
        public string? SelectedText { get; set; }
        public string? Phonetic { get; set; }
        public string? PrefixContext { get; set; }
        public string? SuffixContext { get; set; }
    }
}

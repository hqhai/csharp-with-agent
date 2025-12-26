// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.RubyAnnotation
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UpdateRubyAnnotationCommandModel : BaseCommandModel
    {
        public string? Phonetic { get; set; }
    }
}

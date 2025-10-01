// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.RubyAnnotation
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class CreateRubyAnnotationCommandModel : BaseCommandModel
    {
        public Guid ObjectId { get; set; }
        public EnumObjectType ObjectType { get; set; }
        public string? Text { get; set; }
        public EnumLanguageType LangueType { get; set; }
        public string? SelectedText { get; set; }
        public string? TextNote { get; set; }
        public int StartGrapheme { get; set; }
        public int LengthSelectedText { get; set; }
        public object? Annotations { get; set; }
    }

}

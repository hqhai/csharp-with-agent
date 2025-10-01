// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.Ruby
{
    using System;
    using Fsel.Shared.Enums;

    public class RubyAnnotaionModel
    {
        public Guid Id { get; set; }
        public int StartGrapheme { get; set; }
        public int LengthNote { get; set; }
        public string? SelectedText { get; set; }
        public string? TextNote { get; set; }
        public EnumLanguageType LanguageType { get; set; }
    }
}

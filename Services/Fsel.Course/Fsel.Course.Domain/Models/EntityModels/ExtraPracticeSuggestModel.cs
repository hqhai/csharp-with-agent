// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class ExtraPracticeSuggestModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public EnumExtraPracticeType Type { get; set; }
        public string? InstructionContent { get; set; }
        public string? ImagePath { get; set; }
    }
}

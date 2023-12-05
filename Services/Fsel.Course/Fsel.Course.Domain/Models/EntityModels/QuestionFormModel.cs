// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class QuestionFormModel : BaseModel
    {
        public string? Name { get; set; }
        public EnumQuestionType Type { get; set; }
        public object? Config { get; set; }
    }
}

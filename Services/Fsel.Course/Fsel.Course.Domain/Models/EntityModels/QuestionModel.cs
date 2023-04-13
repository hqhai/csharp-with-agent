// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class QuestionModel : BaseModel
    {
        public EnumQuestionType QuestionType { get; set; }
        public int CorrectTotal { get; set; }
        public bool Ungraded { get; set; }
        public string? Explanation { get; set; }
        public object? Config { get; set; }
        public VideoTimeCodeAnswerModel? VideoTimeCodeAnswer { get; set; }
        public object? Answer { get; set; }
    }
}

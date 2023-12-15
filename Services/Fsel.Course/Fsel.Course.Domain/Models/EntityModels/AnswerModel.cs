// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class AnswerModel
    {
        public int CorrectCount { get; set; }
        public object? Answer { get; set; }
        public bool? IsCorrect { get; set; }
        public EnumSubAnswerStatus? SubAnswerStatus { get; set; }
    }
}

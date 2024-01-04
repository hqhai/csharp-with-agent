// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class AnswerModel
    {
        public int CorrectCount { get; set; }
        public object? Answer { get; set; }
        public bool? IsCorrect { get; set; }

        public EnumCorrectStatus? SubAnswerStatus
        {
            get
            {
                return IsCorrect.HasValue ? IsCorrect.Value ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail : EnumCorrectStatus.Process;
            }
        }

        public EnumAnswerStatus Status { get; set; }
    }
}

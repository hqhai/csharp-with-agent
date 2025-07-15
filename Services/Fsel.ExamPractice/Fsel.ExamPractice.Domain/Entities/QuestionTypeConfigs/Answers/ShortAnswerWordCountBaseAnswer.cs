// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Answers
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class ShortAnswerWordCountBaseAnswer
    {
        public string? Answers { get; set; }
        public bool? IsExact { get; set; }

        public EnumCorrectStatus Status
        {
            get
            {
                return IsExact.HasValue ? IsExact.Value ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail : EnumCorrectStatus.Process;
            }
        }

        public int WordCount => StringHelper.CountWords(Answers);
        public bool IsFirstSubmit { get; set; } = true;
    }
}

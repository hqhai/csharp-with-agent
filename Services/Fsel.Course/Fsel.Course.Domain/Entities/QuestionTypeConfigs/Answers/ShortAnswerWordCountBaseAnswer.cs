// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class ShortAnswerWordCountBaseAnswer
    {
        public string? Answers { get; set; }
        public bool? IsExact { get; set; }

        public EnumSubAnswerStatus Status
        {
            get
            {
                return IsExact.HasValue ? IsExact.Value ? EnumSubAnswerStatus.Correct : EnumSubAnswerStatus.Fail : EnumSubAnswerStatus.Process;
            }
        }

        public int WordCount => StringHelper.CountWords(Answers);
        public bool IsFirstSubmit { get; set; } = true;
    }
}

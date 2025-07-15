// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class MultipleOptionSentenceCompletionAnswer
    {
        public IList<MultipleOptionSentenceCompletionAnswers>? Answers { get; set; }
    }

    public class MultipleOptionSentenceCompletionAnswers
    {
        public long Id { get; set; }
        public long? AnswerId { get; set; }
        public bool? IsExact { get; set; }

        public EnumCorrectStatus Status
        {
            get
            {
                return IsExact.HasValue ? IsExact.Value ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail : EnumCorrectStatus.Process;
            }
        }

        public bool IsFirstSubmit { get; set; } = true;
    }
}

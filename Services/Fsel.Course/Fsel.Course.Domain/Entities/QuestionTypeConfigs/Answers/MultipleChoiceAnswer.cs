// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class MultipleChoiceAnswer
    {
        public IList<MultipleChoiceAnswers>? Answers { get; set; }
    }

    public class MultipleChoiceAnswers
    {
        public long Id { get; set; }
        public bool IsChecked { get; set; }
        public bool? IsExact { get; set; }

        public EnumSubAnswerStatus Status
        {
            get
            {
                return IsExact.HasValue ? IsExact.Value ? EnumSubAnswerStatus.Correct : EnumSubAnswerStatus.Fail : EnumSubAnswerStatus.Process;
            }
        }

        public bool IsFirstSubmit { get; set; } = true;
    }
}

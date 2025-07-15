// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class DragAndDropSentenceOrderAnswer
    {
        public IList<DragAndDropSentenceOrderAnswers>? Answers { get; set; }
    }

    public class DragAndDropSentenceOrderAnswers
    {
        public long Id { get; set; }
        public IList<string>? Answer { get; set; }
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

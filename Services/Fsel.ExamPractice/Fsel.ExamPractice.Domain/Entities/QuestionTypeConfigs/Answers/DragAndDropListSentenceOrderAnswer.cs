// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class DragAndDropListSentenceOrderAnswer
    {
        public IList<DragAndDropListSentenceOrderAnswers> Answers { get; set; } = new List<DragAndDropListSentenceOrderAnswers>();
    }

    public class DragAndDropListSentenceOrderAnswers
    {
        public long Id { get; set; }
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

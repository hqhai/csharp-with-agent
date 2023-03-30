// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Answers
{
    using System.Collections.Generic;

    public class DragAndDropSentenceOrderAnswer
    {
        public IList<DragAndDropSentenceOrderAnswers>? Answers { get; set; }
    }

    public class DragAndDropSentenceOrderAnswers
    {
        public int Id { get; set; }

        public string? Answer { get; set; }

        public bool IsExact { get; set; }
    }
}

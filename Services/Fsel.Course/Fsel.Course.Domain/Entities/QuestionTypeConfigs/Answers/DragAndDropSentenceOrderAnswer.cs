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
        public long Id { get; set; }
        public IList<string>? Answer { get; set; }
        public bool? IsExact { get; set; }
        public bool? IsExactDisplay { get; set; }
    }
}

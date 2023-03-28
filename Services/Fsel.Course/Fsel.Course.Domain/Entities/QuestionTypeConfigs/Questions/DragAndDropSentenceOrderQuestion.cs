// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;

    public class DragAndDropSentenceOrderQuestion
    {
        public string? Name { get; set; }

        public IList<DragAndDropSentenceOrderQuestionContent>? Contents { get; set; }
    }

    public class DragAndDropSentenceOrderQuestionContent
    {
        public int Id { get; set; }
        public IList<string>? Words { get; set; }
    }
}

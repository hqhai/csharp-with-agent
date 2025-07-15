// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class DragAndDropListSentenceOrderQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public IList<DragAndDropListSentenceOrderQuestionContent>? Contents { get; set; }
    }

    public class DragAndDropListSentenceOrderQuestionContent
    {
        public long Id { get; set; }

        [JsonRequired]
        public string? Content { get; set; }
    }
}

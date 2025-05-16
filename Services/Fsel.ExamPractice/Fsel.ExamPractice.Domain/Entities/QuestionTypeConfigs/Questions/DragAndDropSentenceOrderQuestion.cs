// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class DragAndDropSentenceOrderQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public IList<DragAndDropSentenceOrderQuestionContent>? Contents { get; set; }
    }

    public class DragAndDropSentenceOrderQuestionContent
    {
        public long Id { get; set; }

        [JsonRequired]
        public IList<string>? Words { get; set; }
    }
}

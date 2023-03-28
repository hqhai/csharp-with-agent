// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    public class ShortAnswerQuestionWordCountBaseQuestion
    {
        public string? Name { get; set; }

        public int? ExactWordCount { get; set; }

        public bool? IsSpeakRequired { get; set; }
    }
}

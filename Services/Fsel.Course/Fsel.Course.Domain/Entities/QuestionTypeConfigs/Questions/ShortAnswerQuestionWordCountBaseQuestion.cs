// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Text.Json.Serialization;

    public class ShortAnswerQuestionWordCountBaseQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public int? ExactWordCount { get; set; }

        [JsonRequired]
        public bool? IsSpeakRequired { get; set; }
    }
}

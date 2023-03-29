// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Text.Json.Serialization;

    public class ShortAnswerQuestionWordBaseQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public string? Content { get; set; }

        public bool IsSpeakRequired { get; set; }
    }
}

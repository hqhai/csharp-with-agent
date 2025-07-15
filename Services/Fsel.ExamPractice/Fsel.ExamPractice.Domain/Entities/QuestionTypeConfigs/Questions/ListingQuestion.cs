// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Text.Json.Serialization;

    public class ListingQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public long? ExactWordCount { get; set; }
    }
}

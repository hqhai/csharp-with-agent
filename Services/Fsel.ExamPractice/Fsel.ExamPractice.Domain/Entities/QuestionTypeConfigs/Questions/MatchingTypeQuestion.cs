// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class MatchingTypeQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public IList<MatchingTypeQuestionContent>? From { get; set; }

        [JsonRequired]
        public IList<MatchingTypeQuestionContent>? To { get; set; }

        [JsonRequired]
        public IList<MatchingTypeQuestionLink>? Link { get; set; }
    }

    public class MatchingTypeQuestionContent
    {
        public long Id { get; set; }

        [JsonRequired]
        public string? Content { get; set; }
    }

    public class MatchingTypeQuestionLink
    {
        public long? FromId { get; set; }

        public long? ToId { get; set; }
    }
}

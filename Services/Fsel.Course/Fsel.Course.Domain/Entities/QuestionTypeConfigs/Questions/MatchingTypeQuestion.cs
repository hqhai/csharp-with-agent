// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
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
        public IList<MatchingTypeQuestionContent>? Tos { get; set; }

        [JsonRequired]
        public IList<MatchingTypeQuestionLink>? Links { get; set; }
    }

    public class MatchingTypeQuestionContent
    {
        public int Id { get; set; }

        [JsonRequired]
        public string? Content { get; set; }
    }

    public class MatchingTypeQuestionLink
    {
        public int FromId { get; set; }

        public int Told { get; set; }
    }
}

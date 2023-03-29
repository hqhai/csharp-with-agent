// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class MutipleChoiceQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public IList<MutipleChoiceQuestionContent>? Contents { get; set; }
    }

    public class MutipleChoiceQuestionContent
    {
        public int Id { get; set; }

        [JsonRequired]
        public string? Content { get; set; }

        [JsonRequired]
        public bool? IsCorrect { get; set; }
    }
}

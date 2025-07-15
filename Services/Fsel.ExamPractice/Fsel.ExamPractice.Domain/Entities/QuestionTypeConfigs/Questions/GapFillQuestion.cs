// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class GapFillQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public IList<GapFillScoreBySubQuesionContent>? Contents { get; set; }
    }

    public class GapFillScoreBySubQuesionContent
    {
        public long Id { get; set; }

        [JsonRequired]
        public string? Content { get; set; }

        [JsonRequired]
        public IList<string>? Words { get; set; }
    }
}

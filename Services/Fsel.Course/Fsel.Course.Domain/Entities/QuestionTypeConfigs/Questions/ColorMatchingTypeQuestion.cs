// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Fsel.Shared.Enums;

    public class ColorMatchingTypeQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public EnumColorMatchingDisplayType Type { get; set; }

        [JsonRequired]
        public IList<ColorMatchingTypeQuestionOption>? Contents { get; set; }

        [JsonRequired]
        public IList<string>? CorrectAnswers { get; set; }
    }

    public class ColorMatchingTypeQuestionOption
    {
        [JsonRequired]
        public long Id { get; set; }

        public string? Text { get; set; }

        public string? ImageUrl { get; set; }

        public string? Description { get; set; }
    }
}

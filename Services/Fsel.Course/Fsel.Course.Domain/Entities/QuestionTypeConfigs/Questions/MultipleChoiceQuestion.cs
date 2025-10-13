// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class MultipleChoiceQuestion : IConfigRuby
    {
        [JsonRequired]
        public string? Name { get; set; }

        public string? NameRuby { get; set; }
        public string? InstructionRuby { get; set; }
        public string? PopupRuby { get; set; }

        [JsonRequired]
        public IList<MultipleChoiceQuestionContent>? Contents { get; set; }
    }

    public class MultipleChoiceQuestionContent
    {
        public long Id { get; set; }
        public string? FilePath { get; set; }

        [JsonRequired]
        public string? Content { get; set; }

        [JsonRequired]
        public bool? IsCorrect { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Text.Json.Serialization;

    public class ShortAnswerQuestionWordBaseQuestion : IConfigRuby
    {
        [JsonRequired]
        public string? Name { get; set; }

        public string? NameRuby { get; set; }
        public string? InstructionRuby { get; set; }
        public string? PopupRuby { get; set; }

        [JsonRequired]
        public IList<string>? Content { get; set; }

        public bool IsSpeakRequired { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class DragAndDropSentenceOrderQuestion : IConfigRuby
    {
        [JsonRequired]
        public string? Name { get; set; }

        public string? NameRuby { get; set; }
        public string? InstructionRuby { get; set; }
        public string? PopupRuby { get; set; }

        [JsonRequired]
        public IList<DragAndDropSentenceOrderQuestionContent>? Contents { get; set; }
    }

    public class DragAndDropSentenceOrderQuestionContent
    {
        public long Id { get; set; }

        [JsonRequired]
        public IList<string>? Words { get; set; }
    }
}

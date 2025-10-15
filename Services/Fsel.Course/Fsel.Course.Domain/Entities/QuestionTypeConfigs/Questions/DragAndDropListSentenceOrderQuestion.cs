// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class DragAndDropListSentenceOrderQuestion : IConfigRuby
    {
        [JsonRequired]
        public string? Name { get; set; }

        public string? NameRuby { get; set; }
        public string? InstructionRuby { get; set; }
        public string? PopupRuby { get; set; }

        [JsonRequired]
        public IList<DragAndDropListSentenceOrderQuestionContent>? Contents { get; set; }
    }

    public class DragAndDropListSentenceOrderQuestionContent
    {
        public long Id { get; set; }

        [JsonRequired]
        public string? Content { get; set; }
    }
}

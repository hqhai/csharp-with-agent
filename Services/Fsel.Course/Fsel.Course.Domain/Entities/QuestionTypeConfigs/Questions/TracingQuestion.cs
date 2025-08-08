// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Text.Json.Serialization;

    public class TracingQuestion
    {
        [JsonRequired]
        public string? Question { get; set; }

        public string? Instruction { get; set; }

        [JsonRequired]
        public Guid KeyboardTextId { get; set; }

        public KeyboardTextModel? KeyboardText { get; set; }

        public string? Popup { get; set; }
    }

    public class KeyboardTextModel
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public int Unicode { get; set; }

        public string? FilePath { get; set; }
    }
}

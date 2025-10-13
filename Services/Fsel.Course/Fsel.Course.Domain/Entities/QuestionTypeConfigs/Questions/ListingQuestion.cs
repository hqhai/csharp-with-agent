// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Text.Json.Serialization;

    public class ListingQuestion : IConfigRuby
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public long? ExactWordCount { get; set; }

        public string? NameRuby { get; set; }
        public string? InstructionRuby { get; set; }
        public string? PopupRuby { get; set; }
    }
}

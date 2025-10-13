// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Text.Json.Serialization;

    public class ExercisePreparationQuestion : IConfigRuby
    {
        [JsonRequired]
        public string? Content { get; set; }

        [JsonRequired]
        public string? ImagePath { get; set; }

        public string? NameRuby { get; set; }
        public string? InstructionRuby { get; set; }
        public string? PopupRuby { get; set; }
    }
}

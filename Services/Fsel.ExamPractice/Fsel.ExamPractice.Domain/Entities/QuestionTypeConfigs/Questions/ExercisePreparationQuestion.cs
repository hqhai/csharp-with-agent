// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Text.Json.Serialization;

    public class ExercisePreparationQuestion
    {
        [JsonRequired]
        public string? Content { get; set; }

        [JsonRequired]
        public string? ImagePath { get; set; }
    }
}

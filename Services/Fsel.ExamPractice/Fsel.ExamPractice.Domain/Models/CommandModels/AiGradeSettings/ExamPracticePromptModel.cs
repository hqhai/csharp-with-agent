// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.CommandModels.AiGradeSettings
{
    using Fsel.Shared.Enums;

    public class ExamPracticePromptModel
    {
        public string? PromptContent { get; set; }
        public EnumMockTestAIType Type { get; set; }
    }
}

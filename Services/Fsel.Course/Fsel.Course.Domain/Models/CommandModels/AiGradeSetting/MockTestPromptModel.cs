// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.AiGradeSetting
{
    using Fsel.Shared.Enums;

    public class MockTestPromptModel
    {
        public string? PromptContent { get; set; }

        public EnumMockTestAIType Type { get; set; }
    }

    public class TestPromptModel
    {
        public string? PromptContent { get; set; }

        public EnumMockTestAIType Type { get; set; }
    }
}

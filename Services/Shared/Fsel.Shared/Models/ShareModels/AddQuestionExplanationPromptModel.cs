// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class AddQuestionExplanationPromptModel
    {
        public Guid QuestionId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public EnumQuestionType QuestionType { get; set; }
        public object? Config { get; set; }
        public string? Explanation { get; set; }
        public string? PromptRequest { get; set; }
        public string? PromptResponse { get; set; }
    }
}

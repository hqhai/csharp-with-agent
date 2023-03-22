// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Commands.SurveyQuestionCmd
{
    using Fsel.Common.Enums;

    public class CreateSurveyQuestionCommand
    {
        public string? Question { get; set; }
        public EnumSurveyQuestion Type { get; set; }
        public string? Answers { get; set; }
    }
}

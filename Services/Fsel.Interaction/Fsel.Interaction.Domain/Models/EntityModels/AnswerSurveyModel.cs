// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    public class AnswerSurveyModel
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }
        public bool IsOther { get; set; }
        public string? Other { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;

    public class CustomerSurvey : Entity
    {
        public string? AnswerStr { get; set; }

        [NotMapped]
        public object? Answer
        {
            get => string.IsNullOrEmpty(AnswerStr) ? null : ConvertHelper.Deserialize<object>(AnswerStr);
            set => AnswerStr = value == null ? null : ConvertHelper.Serialize(value);
        }

        public Guid UserId { get; set; }

        public bool IsCompleted { get; set; }

        public Guid SurveyQuestionId { get; set; }

        public Guid? CustomerSurveyGroupId { get; set; }

        public SurveyQuestion? SurveyQuestion { get; set; }

        public CustomerSurveyGroup? CustomerSurveyGroup { get; set; }
    }
}

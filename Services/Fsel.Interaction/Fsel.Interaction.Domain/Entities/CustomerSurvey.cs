// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class CustomerSurvey : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Answers { get; set; }

        public Guid UserId { get; set; }
        public SurveyQuestion? SurveyQuestion { get; set; }
        public Guid SurveyQuestionId { get; set; }
    }
}

// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Core.Base.Interfaces;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;

namespace Fsel.Interaction.Domain.Entities
{
    public class SurveyQuestion : Entity, IMultiLingualObject<SurveyQuestionTranslation>
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Question { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Title { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Icon { get; set; }

        public float DisplayOrder { get; set; }

        public int DisplayLevel { get; set; }

        public EnumSurveyFormType SurveyFormType { get; set; }

        public bool? IsPilot { get; set; }

        public bool? IsRequired { get; set; }

        public EnumSurveyQuestion Type { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? AnswerStr { get; set; }

        [NotMapped]
        public object? Answers
        {
            get { return ConvertHelper.Deserialize<object>(AnswerStr); }
            set { AnswerStr = ConvertHelper.Serialize(value); }
        }

        public Guid? CompetitionEventId { get; set; }

        public Guid? SurveyConfigId { get; set; }
        public SurveyConfig? SurveyConfig { get; set; }

        public IList<CustomerSurvey> CustomerSurveys { get; set; } = new List<CustomerSurvey>();

        public ICollection<SurveyQuestionTranslation> Translations { get; set; } = new List<SurveyQuestionTranslation>();
    }

    public class SurveyQuestionTranslation : Entity, ITranslationObject
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Question { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? AnswerStr { get; set; }

        [NotMapped]
        public object? Answers
        {
            get { return ConvertHelper.Deserialize<object>(AnswerStr); }
            set { AnswerStr = ConvertHelper.Serialize(value); }
        }

        public Guid SurveyQuestionId { get; set; }

        public SurveyQuestion? SurveyQuestion { get; set; }

        public string? Language { get; set; }
    }
}

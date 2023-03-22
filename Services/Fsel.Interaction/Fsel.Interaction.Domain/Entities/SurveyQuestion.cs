// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Common.Helpers;
using Fsel.Core.Entities;

namespace Fsel.Interaction.Domain.Entities
{
    public class SurveyQuestion : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Question { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Icon { get; set; }

        public int DisplayOrder { get; set; }

        public EnumSurveyQuestion Type { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? AnswerStr { get; set; }

        [NotMapped]
        public object? Answers
        {
            get { return ConvertHelper.Deserialize<object>(AnswerStr); }
            set { AnswerStr = ConvertHelper.Serialize(value); }
        }

        public IList<CustomerSurvey> CustomerSurveys { get; set; } = new List<CustomerSurvey>();
    }
}

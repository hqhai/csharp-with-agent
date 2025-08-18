// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums;

    public class ColorMatchingTypeQuestion : ValidationEntity
    {
        [JsonRequired]
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Câu hỏi không được chứa ký tự '<' hoặc '>'.")]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        public int Trial { get; set; } = 4;

        [JsonRequired]
        public EnumColorMatchingDisplayType Type { get; set; }

        [JsonRequired]
        public IList<ColorMatchingTypeQuestionOption> Contents { get; set; } = new List<ColorMatchingTypeQuestionOption>();
    }

    public class ColorMatchingTypeQuestionOption : ValidationEntity
    {
        [JsonRequired]
        public long Id { get; set; }

        [RegularExpression(@"^[^<>]*$", ErrorMessage = "Câu hỏi không được chứa ký tự '<' hoặc '>'.")]
        [MaxLength(50, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Text { get; set; }

        public string? ImageUrl { get; set; }

        [JsonRequired]
        public bool? IsCorrect { get; set; }
    }
}

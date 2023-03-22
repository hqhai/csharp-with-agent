// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs.QuestionAnswers
{
    public class YourPositionQuestion
    {
    }

    public class YourPositionAnswer
    {
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public string? ProvinceCode { get; set; }
        public string? ProvinceName { get; set; }
    }
}

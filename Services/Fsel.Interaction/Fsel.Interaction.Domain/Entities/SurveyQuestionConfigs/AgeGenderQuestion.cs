// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs
{
    using Fsel.Shared.Enums;

    public class AgeGenderQuestion
    {
        public DateTime? Birthday { get; set; }

        public IList<AgeGenderQuestionAnswers>? AgeGenderQuestions { get; set; }
    }

    public class AgeGenderQuestionAnswers
    {
        public int Id { get; set; }
        public EnumGender Gender { get; set; }
    }

    public class AgeGenderAnswer
    {
        public DateTime? Birthday { get; set; }

        public AgeGenderQuestionAnswers? AgeGenderAnswers { get; set; }
    }
}
